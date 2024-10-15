using QuartzScheduler.Common.Exceptions;
using QuartzScheduler.Common.Services;
using System;
using System.Collections.Concurrent;

namespace QuartzScheduler.Core.Services;

public class ExternalIdMemoryStore : IExternalIdRepository
{
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public void Add(Guid appId, string objId, string jobKey)
    {
        if (!_cache.TryAdd($"{appId:N}_{objId}", jobKey))
            throw new ArgumentException($"Schedule for application {appId} and object {objId} already exists");
    }

    public void Delete(Guid appId, string objId)
    {
        _cache.TryRemove($"{appId:N}_{objId}", out string _);
    }

    public bool DoesJobKeyExist(Guid appId, string objId)
    {
        return _cache.TryGetValue($"{appId:N}_{objId}", out string _);
    }

    public string GetJobKey(Guid appId, string objId)
    {
        if (!_cache.TryGetValue($"{appId:N}_{objId}", out string jobKey))
            throw new NotFoundException($"Not found schedule for application {appId} and object {objId}");

        return jobKey;
    }
}
