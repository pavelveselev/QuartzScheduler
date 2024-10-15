using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuartzScheduler.Dal.Tables;

namespace QuartzScheduler.Dal.DbConfigurations;

public class JobToObjectsConfiguration : IEntityTypeConfiguration<JobToObject>
{
    public void Configure(EntityTypeBuilder<JobToObject> builder)
    {
        builder.HasKey(j => j.Id);

        builder
            .Property(j => j.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(j => j.JobName)
            .HasMaxLength(150);

        builder
            .Property(j => j.JobGroup)
            .HasMaxLength(150);

        builder
            .Property(j => j.ObjectId)
            .HasMaxLength(100);

        builder
            .HasIndex(j => new { j.ApplicationId, j.ObjectId });
    }
}
