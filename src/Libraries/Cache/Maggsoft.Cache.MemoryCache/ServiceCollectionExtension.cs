using Microsoft.Extensions.DependencyInjection;
using System;

namespace Maggsoft.Cache.MemoryCache
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMaggsoftDistributedMemoryCache(
            this IServiceCollection services, params Type[] assemblyPointerTypes)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICache, MaggsoftDistributedCache>();
            services.DecorateAllInterfacesUsingAspect(assemblyPointerTypes);
            return services;
        }
    }
}