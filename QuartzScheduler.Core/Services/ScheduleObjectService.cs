using Microsoft.Extensions.Logging;
using QuartzScheduler.Common.Exceptions;
using QuartzScheduler.Common.Models;
using QuartzScheduler.Common.Services;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzScheduler.Core.Services;

public class ScheduleObjectService : IScheduleObjectService
{
    private readonly ILogger<ScheduleObjectService> _logger;
    private readonly IScheduleJobService _scheduleJobService;
    private readonly IExternalIdRepository _externalIdRepository;

    public ScheduleObjectService(
        ILogger<ScheduleObjectService> logger,
        IScheduleJobService scheduleJobService,
        IExternalIdRepository externalIdRepository)
    {
        _logger = logger;
        _scheduleJobService = scheduleJobService;
        _externalIdRepository = externalIdRepository;
    }

    public async Task CreateScheduleObjectAsync<T>(Guid appId, string id,
        ScheduleCreationModel<T> scheduleCreationModel, CancellationToken cancellationToken)
    {
        if (_externalIdRepository.DoesJobKeyExist(appId, id))
            throw new AlreadyExistsException($"Schedule task with application id {appId}, object id {id} already exists");

        try
        {
            var key = await _scheduleJobService.CreateNotificationJobAsync(scheduleCreationModel, cancellationToken);

            _logger.LogInformation($"App id {appId} object id {id}: Task with key {key} is created");

            try
            {
                _externalIdRepository.Add(appId, id, key);
            }
            catch (Exception ex)
            {
                await _scheduleJobService.DeleteJobAsync(key, cancellationToken);
                _logger.LogError(ex, $"Id repository error. App id {appId}, object id {id}. Task with key {key} is deleted");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating scheduleTask: app id {appId}, object id {id}, model {JsonSerializer.Serialize(scheduleCreationModel)}", ex);
            throw;
        }
    }

    public async Task DeleteScheduleAsync(Guid appId, string id, CancellationToken cancellationToken)
    {
        var jobKey = _externalIdRepository.GetJobKey(appId, id);

        try
        {
            await _scheduleJobService.DeleteJobAsync(jobKey, cancellationToken);
            _externalIdRepository.Delete(appId, id);
        }
        catch (NotFoundException)
        {
            _externalIdRepository.Delete(appId, id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting scheduleTask: app id {appId}, object id {id}, job key {jobKey}", ex);
        }
    }

    public async Task<bool> DoesScheduleExistsAsync(Guid appId, string id, CancellationToken cancellationToken)
    {
        try
        {
            var jobKey = _externalIdRepository.GetJobKey(appId, id);
            try
            {
                var jobDetails = await _scheduleJobService.GetJobDetailsAsync(jobKey, cancellationToken);

                return jobDetails != null;
            }
            catch (NotFoundException)
            {
                _externalIdRepository.Delete(appId, id);
                return false;
            }
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    public async Task<ScheduleDetails> GetScheduleDetailsAsync(Guid appId, string id, CancellationToken cancellationToken)
    {
        var jobKey = _externalIdRepository.GetJobKey(appId, id);

        return await _scheduleJobService.GetJobDetailsAsync(jobKey, cancellationToken).ConfigureAwait(false);
    }
}
