using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Maggsoft.Cache.Redis; 

public class RedisDistributedCache( 
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RedisCacheOptions> redisCacheOptions) : ICache
{
    private readonly IDatabase _cache = connectionMultiplexer.GetDatabase();
    private readonly IDistributedCache _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly string _instanceName = redisCacheOptions.Value.InstanceName ?? string.Empty;

    public object Get(string cacheKey)
        => Get(cacheKey, typeof(object));

    public object Get(string cacheKey, Type deserializeType)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        if (deserializeType == null)
            throw new ArgumentNullException(nameof(deserializeType));

        byte[] cacheData = _distributedCache.Get(cacheKey);
        if (cacheData == null)
            return null;

        return MessagePackSerializer.Deserialize(deserializeType, cacheData, ContractlessStandardResolver.Options);
    }

    public async Task<object> GetAsync(string cacheKey)
        => await GetAsync(cacheKey, typeof(object));

    public async Task<object> GetAsync(string cacheKey, Type deserializeType)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        if (deserializeType == null)
            throw new ArgumentNullException(nameof(deserializeType));

        byte[] cacheData = await _distributedCache.GetAsync(cacheKey);
        if (cacheData == null)
            return null;

        return MessagePackSerializer.Deserialize(deserializeType, cacheData, ContractlessStandardResolver.Options);
    }

    public T Get<T>(string cacheKey)
        => (T)Get(cacheKey, typeof(T));

    public T Get<T>(string cacheKey, TimeSpan cacheTime, Func<T> acquire)
    {
        var result = Get<T>(cacheKey);
        if (result != null)
        {
            return result;
        }

        var newData = acquire();
        Set(cacheKey, cacheTime, true, newData);
        return newData;
    }

    public async Task<T> GetAsync<T>(string cacheKey)
    {
        var cacheResult = await GetAsync(cacheKey, typeof(T));
        if (cacheResult == null)
            return default;

        return (T)cacheResult;
    }

    public async Task<T> GetAsync<T>(string cacheKey, TimeSpan cacheTime, Func<Task<T>> acquire)
    {
        var result = await GetAsync<T>(cacheKey);
        if (result != null)
        {
            return result;
        }

        var newData = await acquire();
        await SetAsync(cacheKey, cacheTime, true, newData);
        return newData;
    }

    public void Set(string cacheKey, TimeSpan duration, bool slidingExpiration, object data)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        byte[] cacheData = MessagePackSerializer.Serialize(data, ContractlessStandardResolver.Options);

        _distributedCache.Set(cacheKey, cacheData, new DistributedCacheEntryOptions()
        {
            SlidingExpiration = slidingExpiration ? duration : (TimeSpan?)null,
            AbsoluteExpiration = !slidingExpiration ? DateTimeOffset.Now + duration : (DateTimeOffset?)null
        });
    }

    public async Task SetAsync(string cacheKey, TimeSpan duration, bool slidingExpiration, object data)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        byte[] cacheData = MessagePackSerializer.Serialize(data, ContractlessStandardResolver.Options);

        await _distributedCache.SetAsync(cacheKey, cacheData, new DistributedCacheEntryOptions()
        {
            SlidingExpiration = slidingExpiration ? duration : (TimeSpan?)null,
            AbsoluteExpiration = !slidingExpiration ? DateTimeOffset.Now + duration : (DateTimeOffset?)null
        });
    }

    public void Refresh(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        _distributedCache.Refresh(cacheKey);
    }

    public async Task RefreshAsync(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        await _distributedCache.RefreshAsync(cacheKey);
    }

    public void Remove(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        _distributedCache.Remove(cacheKey);
    }

    public async Task RemoveAsync(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        await _distributedCache.RemoveAsync(cacheKey);
    }

    public void RemoveByPattern(string cachePattern)
    {
        foreach (var endpoint in _connectionMultiplexer.GetEndPoints(true))
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            var keys = server.Keys(database: _cache.Database, pattern: _instanceName + cachePattern).ToArray();
            _cache.KeyDeleteAsync(keys);
        }
    }

    public async Task RemoveByPatternAsync(string cachePattern)
    {
        foreach (var endpoint in _connectionMultiplexer.GetEndPoints(true))
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            var keys = server.Keys(database: _cache.Database, pattern: _instanceName + cachePattern).ToArray();
            await _cache.KeyDeleteAsync(keys);
        }
    }

    public void Clear()
    {
        foreach (var endpoint in _connectionMultiplexer.GetEndPoints(true))
        {
            _connectionMultiplexer.GetServer(endpoint).FlushDatabase(_cache.Database);
        }
    }

    public async Task ClearAsync()
    {
        foreach (var endpoint in _connectionMultiplexer.GetEndPoints(true))
        {
            await _connectionMultiplexer.GetServer(endpoint).FlushDatabaseAsync(_cache.Database);
        }
    }
}
