using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace QuartzScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiVersionNeutral]
    public class TimeController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return $"UTC : {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss \"GMT\"zzz}{Environment.NewLine}Local: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss \"GMT\"zzz}";
        }

        [HttpGet("timezones")]
        public string GetTimezones()
        {
            return string.Join(Environment.NewLine, TimeZoneInfo.GetSystemTimeZones().Select(z => $"{z.BaseUtcOffset} {z.Id} {z.DisplayName}"));
        }
    }
}
