using System;

namespace QuartzScheduler.Common.Models
{
    public class ScheduleDetails
    {
        public string Key { get; set; }

        public string CronExpression { get; set; }

        public DateTimeOffset StartTimeUtc { get; set; }

        public DateTimeOffset? EndTimeUtc { get; set; }

        public DateTimeOffset? FinalFireTimeUtc { get; set; }

        public string Description { get; set; }

        public string Notification { get; set; }
    }
}
