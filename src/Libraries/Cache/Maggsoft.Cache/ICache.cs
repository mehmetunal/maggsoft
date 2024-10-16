using System;
using System.Threading.Tasks;

namespace Maggsoft.Cache;
 
public interface ICache     
{
    object Get(string cacheKey);
    object Get(string cacheKey, Type deserializeType);

    Task<object> GetAsync(string cacheKey);
    Task<object> GetAsync(string cacheKey, Type deserializeType);

    T Get<T>(string cacheKey);
    T Get<T>(string cacheKey, TimeSpan cacheTime, Func<T> acquire);
    Task<T> GetAsync<T>(string cacheKey);
    Task<T> GetAsync<T>(string cacheKey, TimeSpan cacheTime, Func<Task<T>> acquire);

    void Set(string cacheKey, TimeSpan duration, bool slidingExpiration, object data);
    Task SetAsync(string cacheKey, TimeSpan duration, bool slidingExpiration, object data);

    void Refresh(string cacheKey);
    Task RefreshAsync(string cacheKey);

    void Remove(string cacheKey);
    //ICache Remove(string cacheKey);
    Task RemoveAsync(string cacheKey);

    void RemoveByPattern(string cachePattern);
    Task RemoveByPatternAsync(string cachePattern);

    void Clear();
    Task ClearAsync();
}