using System;
using System.Collections.Generic;

namespace QuartzScheduler.Api.Models
{
    public class ScheduleCreationRequest
    {
        public string CronExpression { get; set; }

        public DateTimeOffset? StartDateTime { get; set; }

        public DateTimeOffset? EndDateTime { get; set; }

        public TimeSpan? TimeZoneOffset { get; set; }

        public NotificationObject Notification { get; set; }
    }

    public class NotificationObject
    {
        public string Uri { get; set; }

        public string Method { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }
    }
}
