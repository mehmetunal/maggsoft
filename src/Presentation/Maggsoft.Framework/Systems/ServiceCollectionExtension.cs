using Maggsoft.Core.Exceptions;
using Maggsoft.Core.IoC;
using Maggsoft.Framework.Extensions;
using Maggsoft.Framework.Helper.ModelStateResponseFactory;
using Maggsoft.Framework.Middleware;
using Maggsoft.Framework.Security.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Systems
{
    public static class ServiceCollectionExtension
    {
        static ApiTokenOptions TokenOptions;

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var tokenOptionsConfiguration = configuration.GetSection("TokenOptions");

            services.Configure<ApiTokenOptions>(tokenOptionsConfiguration);

            TokenOptions = tokenOptionsConfiguration.Get<ApiTokenOptions>();

            services.AddControllers().AddJsonOptionsConfig();
            
            services.AddEndpointsApiExplorer();

            services.AddAdminApiCors(TokenOptions);

            services.AddApiVersioningConfig(configuration);

            services.AddHttpContextAccessor();

            services.AddSwaggerGenConfig(TokenOptions);

            services.RegisterAll<IService>();

            //TODO: WepApi de eklenmesi gerek
            //services.AddSingleton<IEventPublisher, EventPublisher>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<ApiBehaviorOptions>(options => { options.InvalidModelStateResponseFactory = ctx => new ModelStateFeatureFilter(); });

            services.AddLogging();

            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddProblemDetails();

            return services;
        }

        public static WebApplication AddInfrastructure(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.Use(async (context, next) =>
                {
                    if (context.Request.IsLocal())
                    {
                        // Forbidden http status code
                        context.Response.StatusCode = 403;
                        return;
                    }

                    await next.Invoke();
                });
            }

            app.UseStaticFiles();

            app.UseSwaggerUIConfig(TokenOptions);

            app.UseRouting();

            app.UseCorsConfig();

            app.UseMiddleware<GlobalResponseHandlingMiddleware>();

            app.UseExceptionHandler(c => c.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                throw exception;
                /*
                  var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                  var response = new { error = exception.InnerException == null ? exception.Message : exception.InnerException.Message };
                  await context.Response.WriteAsJsonAsync(response);
                 */
            }));

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.UseStatusCodePages(new StatusCodePagesOptions()
            {
                HandleAsync = (ctx) =>
                {
                    if (ctx.HttpContext.Response.StatusCode == 404)
                    {
                        throw new NotFoundException($"Not Found Page");
                    }

                    return Task.FromResult(0);
                }
            });

            app.ConfigureRequestPipeline();

            return app;
        }
    }
}