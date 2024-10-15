using System;
using System.Linq;

namespace QuartzScheduler.Common.Extensions
{
    public static class TimezoneHelper
    {
        public static TimeZoneInfo FindTimeZone(this TimeSpan timeZoneOffset)
        {
            var moscowTimeSpan = new TimeSpan(3, 0, 0);
            var systemTimeZones = TimeZoneInfo.GetSystemTimeZones();
            var timezonesWithOffset = systemTimeZones.Where(tz => tz.BaseUtcOffset == timeZoneOffset);
            if (!timezonesWithOffset.Any())
                return null;

            return timezonesWithOffset.FirstOrDefault();
        }
    }
}
