using QuartzScheduler.Common.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzScheduler.Common.Services;

public interface IScheduleObjectService
{
    Task CreateScheduleObjectAsync<T>(Guid appId, string id,
        ScheduleCreationModel<T> scheduleCreationModel, CancellationToken cancellationToken);

    Task DeleteScheduleAsync(Guid appId, string id, CancellationToken cancellationToken);

    Task<bool> DoesScheduleExistsAsync(Guid appId, string id, CancellationToken cancellationToken);

    Task<ScheduleDetails> GetScheduleDetailsAsync(Guid appId, string id, CancellationToken cancellationToken);
}
