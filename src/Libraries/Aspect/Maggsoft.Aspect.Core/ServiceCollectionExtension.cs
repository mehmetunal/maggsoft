using System;
using System.Linq;
using System.Reflection;
using Maggsoft.Aspect.Core.Aspects;
using System.Collections.Generic;
using Maggsoft.Aspect.Core.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Maggsoft.Aspect.Core
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Decorate single interface with AspectDecorator
        /// </summary>
        /// <typeparam name="TInterface">Interface to decore</typeparam>
        public static IServiceCollection DecorateWithAspect<TInterface>(this IServiceCollection services)
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);
            if (!interfaceType.IsInterface)
                throw new ArgumentException("TInterface can only be of interface type!");

            return services.DescribeAll(interfaceType);
        }

        /// <summary>
        /// Decorate all interfaces implemented from the base interface with the AspectDecorator.
        /// </summary>
        /// <param name="services">DI ServiceCollection</param>
        /// <param name="assembly">Assembly to be scanned</param>
        /// <typeparam name="TBaseType">Base interface type</typeparam>
        public static IServiceCollection DecorateAllInterfacesImplemented<TBaseType>(this IServiceCollection services, Assembly assembly)
            where TBaseType : class
        {
            Type baseType = typeof(TBaseType);
            if (!baseType.IsInterface)
                throw new ArgumentException("TBaseType can only be of interface type!");

            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Type[] assignableInterfaceTypes = assembly.GetTypes()
                .Where(t => t.IsInterface &&
                            t != baseType &&
                            baseType.IsAssignableFrom(t))
                .ToArray();

            return services.DescribeAll(assignableInterfaceTypes);
        }

        /// <summary>
        /// Decorate all interfaces including AspectAttribute with the AspectDecorator.
        /// </summary>
        /// <param name="services">DI ServiceCollection</param>
        /// <param name="assembly">Assembly to be scanned</param>
        public static IServiceCollection DecorateAllInterfacesUsingAspect(this IServiceCollection services, Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Type[] assignableInterfaceTypes = assembly.GetTypes()
                .Where(t => t.IsInterface &&
                            t.GetMethods().Any(method => method.GetCustomAttribute<AspectAttribute>() != null))
                .ToArray();

            return services.DescribeAll(assignableInterfaceTypes);
        }

        private static IServiceCollection DescribeAll(this IServiceCollection services, params Type[] serviceTypes)
        {
            if(services == null)
                throw new ArgumentNullException(nameof(services));
            
            foreach (var serviceType in serviceTypes)
            {
                List<ServiceDescriptor> descriptorsToDecorate = services.Where(s => s.ServiceType == serviceType).ToList();
                if (!descriptorsToDecorate.Any())
                    continue;

                MethodInfo createMethod = typeof(AspectDecorator<>)
                    .MakeGenericType(serviceType)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(info => !info.IsGenericMethod && info.ReturnType == serviceType);
            
                foreach (var descriptor in descriptorsToDecorate)
                {
                    ServiceDescriptor decorated = ServiceDescriptor.Describe(serviceType, sp =>
                        {
                            object[] args = new[] { sp.CreateInstance(descriptor), sp };
                            var decoratorInstance = createMethod.Invoke(null, args);
                            
                            return decoratorInstance;
                        },
                        descriptor.Lifetime);

                    services.Remove(descriptor);
                    services.Add(decorated);
                }
            }

            return services;
        }
        
        private static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (descriptor.ImplementationFactory != null)
                return descriptor.ImplementationFactory(services);

            return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType);
        }
    }
}