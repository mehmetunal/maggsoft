using Maggsoft.Aspect.Core;
using Maggsoft.Aspect.Core.Aspects;
using Maggsoft.Cache.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Maggsoft.Cache.Attributes;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
public class CacheInvalidateAttribute: AspectAttribute
{
    private readonly string _cacheKey;
    private readonly Type _targetType;
    private readonly string _targetMethodName;
    
    private ILogger<CacheInvalidateAttribute> _logger;
    private ICache _cache;

    #region Ctors

    public CacheInvalidateAttribute()
    {
        
    }
    
    public CacheInvalidateAttribute(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            throw new ArgumentNullException(nameof(cacheKey));

        _cacheKey = cacheKey;
    }
    
    public CacheInvalidateAttribute(Type targetType)
    {
        if (targetType == null)
            throw new ArgumentNullException(nameof(targetType));

        _targetType = targetType;
    }

    public CacheInvalidateAttribute(Type targetType, string targetMethodName)
        : this(targetType)
    {
        if (string.IsNullOrEmpty(targetMethodName))
            throw new ArgumentNullException(nameof(targetMethodName));

        _targetMethodName = targetMethodName;
    }

    #endregion
    
    public override void OnSuccess(MethodExecutionArgs args)
    {
        string cacheKey = GetCacheName(args);
        _cache.RemoveByPattern(cacheKey);
        
        _logger.LogInformation("Cache invalidated for key: {CacheKey} after invoked method : {MethodName}", cacheKey, args.Method.Name);
    }

    public override async Task OnSuccessAsync(MethodExecutionArgs args)
    {
        string cacheKey = GetCacheName(args);
        await _cache.RemoveByPatternAsync(cacheKey);
        
        _logger.LogInformation("Cache invalidated for key: {CacheKey} after invoked method : {MethodName}", cacheKey, args.Method.Name);
    }

    public override AspectAttribute LoadDependencies(IServiceProvider serviceProvider)
    {
        _cache ??= serviceProvider.GetRequiredService<ICache>();
        if (_cache == null)
            throw new ArgumentException("ICareerIDistributedCache is not registered on DI.");
        
        _logger ??= serviceProvider.GetRequiredService<ILogger<CacheInvalidateAttribute>>();
      
        return base.LoadDependencies(serviceProvider);
    }

    private string GetCacheName(MethodExecutionArgs args)
    {
        if (!string.IsNullOrEmpty(_cacheKey))
            return CacheHelper.GetCacheKey(_cacheKey);

        if (_targetType != null)
            return CacheHelper.GetCacheKey(_targetType, _targetMethodName);

        return CacheHelper.GetCacheKey(args.Method.DeclaringType);
    }
}