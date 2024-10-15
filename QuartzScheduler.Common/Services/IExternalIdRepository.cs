using System;

namespace QuartzScheduler.Common.Services;

public interface IExternalIdRepository
{
    void Add(Guid appId, string objId, string jobKey);

    void Delete(Guid appId, string id);

    bool DoesJobKeyExist(Guid appId, string id);

    string GetJobKey(Guid appId, string objId);
}
