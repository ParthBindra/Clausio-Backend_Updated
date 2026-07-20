using Microsoft.Extensions.Caching.Memory;

namespace Clausio.Legal.Cache;

public class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    public bool TryGet<T>(string key, out T? value) => memoryCache.TryGetValue(key, out value);

    public void Set<T>(string key, T value, TimeSpan ttl) =>
        memoryCache.Set(key, value, ttl);

    public void Remove(string key) => memoryCache.Remove(key);
}
