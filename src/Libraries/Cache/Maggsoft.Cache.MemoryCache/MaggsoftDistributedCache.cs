using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Maggsoft.Cache.MemoryCache;  

public class MaggsoftDistributedCache(IDistributedCache cache, IServiceProvider serviceProvider) : ICache  
{
    #region Properties
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    #endregion

    #region Method
    public object Get(string cacheKey)
       => Get(cacheKey, typeof(object));

    public object Get(string cacheKey, Type deserializeType)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        if (deserializeType == null)
            throw new ArgumentNullException(nameof(deserializeType));

        byte[] cacheData = _cache.Get(cacheKey);
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

        byte[] cacheData = await _cache.GetAsync(cacheKey);
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
            return (T?)default;

        return (T)cacheResult;
    }

    public async Task<T> GetAsync<T>(string cacheKey, TimeSpan cacheTime, Func<Task<T>> acquire)
    {
        var result = await GetAsync<T>(cacheKey);
        if (result != null && !result.Equals((T)default))
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

        _cache.Set(cacheKey, cacheData, new DistributedCacheEntryOptions()
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

        await _cache.SetAsync(cacheKey, cacheData, new DistributedCacheEntryOptions()
        {
            SlidingExpiration = slidingExpiration ? duration : (TimeSpan?)null,
            AbsoluteExpiration = !slidingExpiration ? DateTimeOffset.Now + duration : (DateTimeOffset?)null
        });
    }

    public void Refresh(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        _cache.Refresh(cacheKey);
    }

    public async Task RefreshAsync(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        await _cache.RefreshAsync(cacheKey);
    }

    public void Remove(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        _cache.Remove(cacheKey);
    }

    //public ICache RemoveT(string cacheKey)
    //{
    //    if (string.IsNullOrEmpty(cacheKey))
    //        throw new ArgumentNullException(nameof(cacheKey));

    //    _distributedCache.Remove(cacheKey);
    //    return this;
    //}

    public async Task RemoveAsync(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        await _cache.RemoveAsync(cacheKey);
    }

    public void RemoveByPattern(string cachePattern)
    {
        if (string.IsNullOrEmpty(cachePattern))
            throw new ArgumentNullException(nameof(cachePattern));

        var keyList = GetAllKeysList(cachePattern);
        this.Removes(keyList);
    }

    public Task RemoveByPatternAsync(string cachePattern)
    {
        if (string.IsNullOrEmpty(cachePattern))
            throw new ArgumentNullException(nameof(cachePattern));

        var keyList = GetAllKeysList(cachePattern);

        this.Removes(keyList);

        return Task.FromResult(0);
    }

    public void Clear()
    {
        var keyList = GetAllKeysList();
        this.Removes(keyList);
    }

    public Task ClearAsync()
    {
        var keyList = GetAllKeysList();
        this.Removes(keyList);

        return Task.FromResult(0);
    }
    #endregion

    #region Private
    private void Removes(List<string> keyList)
        => keyList.ForEach((key) => _cache.Remove(key));

    private List<string> GetAllKeysList()
    {
        var items = new List<string>();

        ReadCacheKeys((cacheItemValue) => { items.Add(cacheItemValue.Key.ToString()); });

        return items;
    }

    private List<string> GetAllKeysList(string cachePattern)
    {
        var items = new List<string>();

        if (string.IsNullOrEmpty(cachePattern)) return items;

        ReadCacheKeys((cacheItemValue) =>
        {
            if (cacheItemValue != null && cacheItemValue.Key.ToString().StartsWith(cachePattern.TrimEnd('*')))
                items.Add(cacheItemValue.Key.ToString());
        });

        return items;
    }

    private void ReadCacheKeys(Action<ICacheEntry> call)
    {
        FieldInfo memCache = typeof(MemoryDistributedCache)
            .GetField("_memCache", BindingFlags.NonPublic | BindingFlags.Instance);

        object memCacheValue = memCache.GetValue(_cache);

        PropertyInfo entriesCollection = memCacheValue.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);

        if (entriesCollection != null)
        {
            if (entriesCollection.GetValue(memCacheValue) is ICollection entriesCollectionValue)
            {
                foreach (dynamic cacheItem in entriesCollectionValue)
                {
                    ICacheEntry cacheItemValue = cacheItem.GetType().GetProperty("Value").GetValue(cacheItem, null);
                    call(cacheItemValue);
                }
            }

        }
        else
        {
            var coherentState = memCacheValue.GetType().GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
            var coherentStateValue = coherentState.GetValue(memCacheValue);

            entriesCollection = coherentStateValue.GetType().GetProperty("StringEntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);

            if (entriesCollection.GetValue(coherentStateValue) is ICollection entriesCollectionValue)
            {
                foreach (dynamic cacheItem in entriesCollectionValue)
                {
                    ICacheEntry cacheItemValue = cacheItem.GetType().GetProperty("Value").GetValue(cacheItem, null);
                    call(cacheItemValue);
                }
            }
        }
    }
    #endregion
}