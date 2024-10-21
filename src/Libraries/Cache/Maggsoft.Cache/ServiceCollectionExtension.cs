using System;
using Microsoft.Extensions.DependencyInjection;
using Maggsoft.Aspect.Core;

namespace Maggsoft.Cache;

public static class ServiceCollectionExtension 
{
    public static IServiceCollection DecorateAllInterfacesUsingAspect(this IServiceCollection services, params Type[] assemblyPointerTypes)
    {
        foreach (Type assemblyPointerType in assemblyPointerTypes)
            services.DecorateAllInterfacesUsingAspect(assemblyPointerType.Assembly);

        return services;
    }
}