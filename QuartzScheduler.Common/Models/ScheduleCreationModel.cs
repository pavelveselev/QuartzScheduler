using System;

namespace QuartzScheduler.Common.Models
{
    public class ScheduleCreationModel<T> : ScheduleCreationModel
    {
        public NotificationModel<T> NotificationObject { get; set; }
    }

    public class ScheduleCreationModel
    {
        public string CronExpression { get; set; }

        public DateTimeOffset? StartDateTime { get; set; }

        public DateTimeOffset? EndDateTime { get; set; }

        public TimeSpan? TimeZoneOffset { get; set; }
    }
}
