using QuartzScheduler.Common.Services;
using QuartzScheduler.Dal.Tables;
using QuartzScheduler.Dal;
using System;
using System.Linq;
using QuartzScheduler.Common.Exceptions;

namespace QuartzScheduler.Core.Services;

public class ExternalIdDbStore : IExternalIdRepository
{
    private readonly SchedulerDbContext _schedulerDbContext;

    public ExternalIdDbStore(SchedulerDbContext schedulerDbContext)
    {
        _schedulerDbContext = schedulerDbContext;
    }

    public void Add(Guid appId, string objId, string jobKey)
    {
        _schedulerDbContext.JobToObjects.Add(new JobToObject
        {
            ApplicationId = appId,
            ObjectId = objId,
            JobName = jobKey,
            JobGroup = ScheduleJobService.JobsGroup,
        });
        _schedulerDbContext.SaveChanges();
    }

    private JobToObject? GetRow(Guid appId, string objId) => _schedulerDbContext.JobToObjects
            .FirstOrDefault(j => j.ApplicationId == appId && j.ObjectId == objId);

    public void Delete(Guid appId, string objId)
    {
        var row = GetRow(appId, objId);

        if (row == null)
            return;

        _schedulerDbContext.JobToObjects.Remove(row);
        _schedulerDbContext.SaveChanges();
    }

    public bool DoesJobKeyExist(Guid appId, string objId)
    {
        var row = GetRow(appId, objId);

        return row != null;
    }

    public string GetJobKey(Guid appId, string objId)
    {
        var row = GetRow(appId, objId);

        if (row == null)
            throw new NotFoundException($"Schedule for application id {appId} and object id {objId} not found");

        return row.JobName;
    }
}
