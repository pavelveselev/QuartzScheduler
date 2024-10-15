namespace QuartzScheduler.Dal.Tables;

public class JobToObject
{
    public long Id { get; set; }

    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public Guid ApplicationId { get; set; }

    public string ObjectId { get; set; }
}
