using System;
using Maggsoft.Framework.Middleware;
using Maggsoft.Framework.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Maggsoft.Framework.Extensions;

/// <summary>
/// builder.Services.AddIPFilter(options =>
//{
//    options.MaxRequestsPerMinute = 60;
//    options.WhitelistedIPs = ["127.0.0.1", "::1"];
//});
//app.UseIPFilter();

/// </summary>
public static class IPFilterExtensions
{
    /// <summary>
    /// IP filtreleme servisini ekler
    /// Adds IP filtering service
    /// </summary>
    public static IServiceCollection AddIPFilter(
        this IServiceCollection services,
        Action<IPFilterOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// IP filtreleme middleware'ini ekler
    /// Adds IP filtering middleware
    /// </summary>
    public static IApplicationBuilder UseIPFilter(this IApplicationBuilder app)
    {
        return app.UseMiddleware<IPFilterMiddleware>();
    }
}