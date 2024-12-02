using Maggsoft.Ocelot.Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using MMLib.Ocelot.Provider.AppConfiguration;

namespace Maggsoft.Ocelot.Core; 

public static class ServiceCollectionExtension 
{
    public static IServiceCollection AddOcelotConfig(this IServiceCollection services, IConfiguration Configuration)
    {
        services.AddOcelot(Configuration).AddCacheManager(option => option.WithDictionaryHandle())
            .AddAppConfiguration();
        services.AddSwaggerForOcelot(Configuration); 

        return services;
    }

    public static void UseOcelotConfig(this IApplicationBuilder app) 
    {
        app.UseSwaggerForOcelotUI(opt =>
        {
            opt.PathToSwaggerGenerator = "/swagger/docs";
        });

        app.UseOcelot(new OcelotPipelineConfiguration
        {
            AuthorizationMiddleware = async (httpContext, next) =>
            {
                await OcelotAuthorizationMiddleware.Authorize(httpContext, next);
            }
        }).Wait();
    }
}
