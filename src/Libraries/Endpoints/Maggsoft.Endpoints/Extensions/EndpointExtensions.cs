using Maggsoft.Endpoints.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Maggsoft.Endpoints.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        ServiceDescriptor[] serviceDescriptors = GetAssemblies().SelectMany(s => s.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }
    public static IServiceCollection AddEndpoints<Type>(this IServiceCollection services) where Type : IEndpoint
    {
        ServiceDescriptor[] serviceDescriptors = GetAssemblies()
            .SelectMany((Assembly s) => s.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(Type)))
            .Select(type => ServiceDescriptor.Transient(typeof(Type), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
    public static IApplicationBuilder MapEndpoints<Type>(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null) where Type : IEndpoint
    {
        IEnumerable<Type> endpoints = app.Services.GetRequiredService<IEnumerable<Type>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (Type endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    private static List<Assembly> GetAssemblies()
    {
        var assemblies = new List<Assembly>();
        var dependenciesNames = DependencyContext.Default.RuntimeLibraries.Where(w => w.Type == "project").Select(s => s.Name).ToList();
        var currentDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        foreach (var dependenciesName in dependenciesNames)
        {
            var cda = currentDomainAssemblies.FirstOrDefault(w => !string.IsNullOrEmpty(w.FullName) && w.FullName.Split(",")[0].Equals(dependenciesName, StringComparison.Ordinal));
            if (cda != null)
            {
                assemblies.Add(cda);
            }
            else
            {
                var assembly = Assembly.Load(new AssemblyName(dependenciesName));
                assemblies.Add(assembly);
            }
        }

        return assemblies;
    }
}
