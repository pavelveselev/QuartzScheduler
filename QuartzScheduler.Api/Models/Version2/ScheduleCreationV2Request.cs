using System;
using System.Collections.Generic;

namespace QuartzScheduler.Api.Models.Version2;

public class ScheduleCreationV2Request
{
    public string CronExpression { get; set; }

    public DateTimeOffset? StartDateTime { get; set; }

    public DateTimeOffset? EndDateTime { get; set; }

    public TimeSpan? TimeZoneOffset { get; set; }

    public Attempts Attempts { get; set; }

    public HttpNotificationWithAttempts Notification { get; set; }
}

public class HttpNotificationWithAttempts
{
    public string Uri { get; set; }

    public string Method { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public string Body { get; set; }
}
