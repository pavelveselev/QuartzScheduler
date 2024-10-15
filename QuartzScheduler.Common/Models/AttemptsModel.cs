using System;

namespace QuartzScheduler.Common.Models
{
    public class AttemptsModel
    {
        public int CurrentAttempt { get; set; }

        public TimeSpan Period { get; set; }

        public int MaxCount { get; set; }
    }
}
