using System;

namespace QuartzScheduler.Api.Models
{
    public class GetScheduleResponse
    {
        public string Key { get; set; }

        public string CronExpression { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset? EndTimeUtc { get; set; }

        public DateTimeOffset? FinalFireTimeUtc { get; set; }

        public string Description { get; set; }

        public string Notification { get; set; }
    }
}
