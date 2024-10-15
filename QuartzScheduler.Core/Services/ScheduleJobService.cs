using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Quartz;
using QuartzScheduler.Common.Exceptions;
using QuartzScheduler.Common.Extensions;
using QuartzScheduler.Common.Models;
using QuartzScheduler.Common.Services;
using QuartzScheduler.Common.Validators;
using QuartzScheduler.Core.Quartz.Jobs;

namespace QuartzScheduler.Core.Services
{
    public class ScheduleJobService : IScheduleJobService
    {
        public const string JobsGroup = "JobsGroup";
        private const string TriggersGroup = "TriggersGroup";

        private readonly ILogger<ScheduleJobService> _logger;
        private readonly IMapper _mapper;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ICronModelValidator _cronModelValidator;

        public ScheduleJobService(
            ILogger<ScheduleJobService> logger,
            IMapper mapper,
            ISchedulerFactory schedulerFactory,
            ICronModelValidator cronModelValidator)
        {
            _logger = logger;
            _mapper = mapper;
            _schedulerFactory = schedulerFactory;
            _cronModelValidator = cronModelValidator;
        }

        public async Task<ScheduleDetails> GetJobDetailsAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"Parameter required: {nameof(key)}");

            IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            var jobDetail = await scheduler.GetJobDetail(new JobKey(key, JobsGroup), cancellationToken);
            var triggerDetail = await scheduler.GetTrigger(new TriggerKey($"Trigger_{key}", TriggersGroup), cancellationToken);

            if (jobDetail == null || triggerDetail == null)
                throw new NotFoundException($"Not found job or trigger with key: {key}");
            else
                return new ScheduleDetails
                {
                    Key = key,
                    CronExpression = (triggerDetail is ICronTrigger cronTriggerDetail) ? cronTriggerDetail.CronExpressionString : null,
                    StartTimeUtc = triggerDetail.StartTimeUtc,
                    EndTimeUtc = triggerDetail.EndTimeUtc,
                    FinalFireTimeUtc = triggerDetail.FinalFireTimeUtc,
                    Description = jobDetail.Description,
                    Notification = JsonSerializer.Serialize(jobDetail.JobDataMap)
                };
        }

        public async Task<string> CreateNotificationJobAsync<T>(ScheduleCreationModel<T> schedule, CancellationToken cancellationToken = default)
        {
            _cronModelValidator.CheckIfCronModelIsValid(schedule);

            IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            var jobData = _mapper.Map<JobDataMap>(schedule.NotificationObject);

            var scheduleKey = Guid.NewGuid().ToString("N");

            IJobDetail job = JobBuilder.Create<SendNotificationJob>()
                .WithIdentity(scheduleKey, JobsGroup)
                .UsingJobData(jobData)
                .Build();

            ITrigger trigger = CreateTrigger(scheduleKey, schedule);

            await scheduler.ScheduleJob(job, trigger, cancellationToken);

            _logger.LogInformation($"Created job with key {scheduleKey} with cron expression {schedule.CronExpression}");

            return scheduleKey;
        }

        public async Task DeleteJobAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"Parameter required: {nameof(key)}");

            IScheduler scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            var jobDetail = await scheduler.GetJobDetail(new JobKey(key, JobsGroup), cancellationToken);

            if (jobDetail == null)
                throw new NotFoundException($"Not found job with key: {key}");

            await scheduler.DeleteJob(new JobKey(key, JobsGroup), cancellationToken);

            _logger.LogInformation($"Deleted job with key {key}");
        }

        private static ITrigger CreateTrigger(string scheduleKey, ScheduleCreationModel schedule)
        {
            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity($"Trigger_{scheduleKey}", TriggersGroup);

            if (!string.IsNullOrWhiteSpace(schedule.CronExpression))
            {
                triggerBuilder = schedule.TimeZoneOffset.HasValue
                    ? triggerBuilder
                        .WithCronSchedule(schedule.CronExpression,
                                cb => cb.InTimeZone(schedule.TimeZoneOffset.Value.FindTimeZone()))
                    : triggerBuilder
                        .WithCronSchedule(schedule.CronExpression);
            }

            triggerBuilder = schedule.StartDateTime.HasValue &&
                             schedule.StartDateTime.Value > DateTimeOffset.UtcNow
                ? triggerBuilder.StartAt(schedule.StartDateTime.Value)
                : triggerBuilder.StartNow();

            if (schedule.EndDateTime.HasValue)
                triggerBuilder = triggerBuilder.EndAt(schedule.EndDateTime.Value);

            triggerBuilder.ForJob(scheduleKey, JobsGroup);

            return triggerBuilder.Build();
        }
    }
}
