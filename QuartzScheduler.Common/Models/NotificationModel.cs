namespace QuartzScheduler.Common.Models
{
    public class NotificationModel<T>
    {
        public AttemptsModel Attempts { get; set; }

        public T Notification { get; set; }
}
}
