using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Quartz;
using QuartzScheduler.Common.Exceptions;
using QuartzScheduler.Common.Models;
using QuartzScheduler.Common.Services;

namespace QuartzScheduler.Core.Quartz.Jobs
{
    public class SendNotificationJob : IJob
    {
        private readonly ILogger<SendNotificationJob> _logger;
        private readonly IMapper _mapper;
        private readonly INotificationClient _notificationService;
        private readonly IScheduleJobService _scheduleJobService;

        public SendNotificationJob(
            ILogger<SendNotificationJob> logger,
            IMapper mapper,
            INotificationClient notificationClient,
            IScheduleJobService scheduleJobService)
        {
            _logger = logger;
            _mapper = mapper;
            _notificationService = notificationClient;
            _scheduleJobService = scheduleJobService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"{nameof(SendNotificationJob)} fired. Job Key: {context.Trigger?.JobKey?.Name}. FireTimeUtc: {context.FireTimeUtc}");

            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var notificationObj = _mapper.Map<NotificationModel<HttpNotificationModel>>(dataMap);

                try
                {
                    await _notificationService.SendAsync(notificationObj.Notification, context.CancellationToken);
                }
                catch (NotificationException nex)
                {
                    //var nextAttemptTime = DateTimeOffset.UtcNow.Add(notificationObj.Attempts.Period == TimeSpan.Zero ? TimeSpan.FromSeconds(10) : notificationObj.Attempts.Period);
                    var nextAttemptTime = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(15));
                    _logger.LogError(nex, $"{nameof(SendNotificationJob)} ERROR. Job Key: {context.Trigger?.JobKey?.Name}. FireTimeUtc: {context.FireTimeUtc}. Next attempt time: {nextAttemptTime}");
                    if (notificationObj.Attempts != null)
                    {
                        notificationObj.Attempts.CurrentAttempt++;
                        if ((notificationObj.Attempts.CurrentAttempt < notificationObj.Attempts.MaxCount) || notificationObj.Attempts.MaxCount == 0)
                        {
                            var nextAttemptModel = new ScheduleCreationModel<HttpNotificationModel>
                            {
                                StartDateTime = nextAttemptTime,
                                NotificationObject = notificationObj,
                            };
                            var nextAttemptJobKey = await _scheduleJobService.CreateNotificationJobAsync(nextAttemptModel);
                            _logger.LogError(nex, $"Job Key: {context.Trigger?.JobKey?.Name}. FireTimeUtc: {context.FireTimeUtc}. Created notification job with key {nextAttemptJobKey} at {nextAttemptTime}");
                        }
                        else
                            _logger.LogError(nex, $"Job Key: {context.Trigger?.JobKey?.Name}. FireTimeUtc: {context.FireTimeUtc}. Max attempts value exceeded. Current attempt {notificationObj.Attempts.CurrentAttempt}. Max attempts {notificationObj.Attempts.MaxCount}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SendNotificationJob)} ERROR. Job Key: {context.Trigger?.JobKey?.Name}. FireTimeUtc: {context.FireTimeUtc}");
            }
        }
    }
}
