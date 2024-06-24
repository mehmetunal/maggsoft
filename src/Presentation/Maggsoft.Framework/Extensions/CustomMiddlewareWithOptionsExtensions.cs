using Maggsoft.Framework.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Maggsoft.Framework.Extensions
{
    public static class CustomMiddlewareWithOptionsExtensions
    {
        public static IServiceCollection AddGlobalResponseMiddlewareWithOptions(this IServiceCollection service, Action<IgnoreResponseOption> options = default)
        {
            options ??= (opts => { });
            service.Configure(options);
            return service;
        }
    }
}
