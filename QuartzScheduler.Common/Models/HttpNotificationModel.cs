using System.Collections.Generic;

namespace QuartzScheduler.Common.Models
{
    public class HttpNotificationModel
    {
        public string Uri { get; set; }

        public string Method { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }
    }
}
