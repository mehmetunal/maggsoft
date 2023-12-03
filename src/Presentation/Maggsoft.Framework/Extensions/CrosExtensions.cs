using Maggsoft.Framework.Security.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Maggsoft.Framework.Extensions;

public static class CrosExtensions
{
    public static IServiceCollection AddAdminApiCors(this IServiceCollection services, ApiTokenOptions apiTokenOptions)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    if (apiTokenOptions.CorsAllowAnyOrigin)
                    {
                        builder.AllowAnyOrigin();
                    }
                    else
                    {
                        builder.WithOrigins(apiTokenOptions.CorsAllowOrigins);
                    }

                    builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithHeaders(HeaderNames.AccessControlAllowHeaders, "Content-Type")
                        ;
                });
        });

        return services;
    }
}