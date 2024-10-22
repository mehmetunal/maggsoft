using System;
using System.Linq;
using System.Reflection;
using Maggsoft.Core.Infrastructure;
using Maggsoft.Core.IoC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Maggsoft.Services.Events;

public static class DependencyContainer
{
    /// <summary>
    /// TServices implemented edilmiş servisleri AddScoped ile register eder.
    /// </summary>
    /// <param name="services"></param>
    /// <exception cref="Exception"></exception>
    public static void RegisterEventConsumer(this IServiceCollection services)    
    {
        try
        {
            // Geçerli uygulama alanına yüklenmiş tüm assembly'leri alın
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var consumerTypes = assemblies
                           .SelectMany(assembly => assembly.GetTypes())
                           .Where(type => type.IsClass && !type.IsAbstract &&
                                          type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>)))
                           .ToList();

            foreach (var consumerType in consumerTypes)
            {
                var implementedInterfacesService =
                    ((TypeInfo)consumerType).ImplementedInterfaces.FirstOrDefault();
                if (implementedInterfacesService == null)
                    throw new ArgumentNullException(nameof(implementedInterfacesService));
                //Log Alınacak

                services.AddTransient(implementedInterfacesService, consumerType);
            }

        }
        catch (Exception ex)
        {
            var logger = MaggsoftContext.Current.Resolve<ILogger<Exception>>();
            logger.LogError($"RegisterEventConsumer => {ex.Message}");
            throw ex;
        }
    }
}
