using Maggsoft.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace Maggsoft.Core.IoC;

public static class DependencyContainer
{
    /// <summary>
    /// TServices implemented edilmiş servisleri AddScoped ile register eder.
    /// </summary>
    /// <param name="services"></param>
    /// <exception cref="Exception"></exception>
    /// <typeparam name="TServices"></typeparam>
    public static void RegisterAll<TServices>(this IServiceCollection services)
    {
        try
        {
            var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(TServices).IsAssignableFrom(p) && p.IsClass == true);

            foreach (var serviceType in serviceTypes)
            {
                var implementedInterfacesService =
                    ((TypeInfo)serviceType).ImplementedInterfaces.FirstOrDefault(w =>
                       w.Name.Equals($"I{serviceType.Name}"));
                if (implementedInterfacesService == null)
                    throw new ArgumentNullException(nameof(implementedInterfacesService));
                //Log Alınacak
                services.AddTransient(implementedInterfacesService, serviceType);
            }
        }
        catch (Exception ex)
        {
            var logger = MaggsoftContext.Current.Resolve<ILogger<Exception>>();
            logger.LogError($"RegisterAll => {ex.Message}");
            throw ex;
        }
    }

    /// <summary>
    /// services.RegisterAll<IWageCaGenerator>(new[] { typeof(IWageCaGenerator).Assembly }, ServiceLifetime.Transient);
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serviceCollection"></param>
    /// <param name="assemblies"></param>
    /// <param name="lifetime"></param>
    public static void RegisterAll<T>(this IServiceCollection serviceCollection, Assembly[] assemblies, ServiceLifetime lifetime)
    {
        var typesFromAssemblies = assemblies
            .SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));

        foreach (var type in typesFromAssemblies)
            serviceCollection.Add(new ServiceDescriptor(typeof(T), type, lifetime));
    }
}