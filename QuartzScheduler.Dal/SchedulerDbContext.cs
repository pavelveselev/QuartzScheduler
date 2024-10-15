using Microsoft.EntityFrameworkCore;
using QuartzScheduler.Dal.DbConfigurations;
using QuartzScheduler.Dal.Tables;

namespace QuartzScheduler.Dal;

public class SchedulerDbContext : DbContext
{
    public const string SchedulerDbConnString = "SchedulerDbContext";
    private readonly string _connectionString;

    public SchedulerDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<JobToObject> JobToObjects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
                .UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new JobToObjectsConfiguration());
    }

    public async Task CleanJobsToObjectsAsync(CancellationToken cancellationToken = default)
    {
        var sql = @$"DELETE jo FROM [JobToObjects] jo 
WHERE NOT EXISTS (SELECT 1 FROM [QRTZ_JOB_DETAILS] jd WHERE jd.JOB_GROUP = jo.JobGroup AND jd.JOB_NAME = jo.JobName)";

        await Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
