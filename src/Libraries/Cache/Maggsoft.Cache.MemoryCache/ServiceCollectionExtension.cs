using Microsoft.Extensions.DependencyInjection;
using System;

namespace Maggsoft.Cache.MemoryCache
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDevDistributedMemoryCache(
            this IServiceCollection services, params Type[] assemblyPointerTypes)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IMaggsoftDistributedCache, MaggsoftDistributedCache>();
            services.DecorateAllInterfacesUsingAspect(assemblyPointerTypes);
            return services;
        }
    }
}