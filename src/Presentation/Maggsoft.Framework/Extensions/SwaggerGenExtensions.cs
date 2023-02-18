using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Maggsoft.Framework.Security.Authorization;
using Maggsoft.Framework.Security.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Maggsoft.Framework.Extensions
{
    public static class SwaggerGenExtensions
    {
        /// <summary>
        /// AddSwaggerGenConfig
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerGenConfig(this IServiceCollection services, ApiTokenOptions apiTokenOptions)
        {
            // services.AddSwaggerGen(c => { c.SwaggerDoc(Version, new OpenApiInfo { Title = Title, Version = Version }); });


            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(apiTokenOptions.ApiVersion, new OpenApiInfo { Title = apiTokenOptions.ApiName, Version = apiTokenOptions.ApiVersion });
                if (!string.IsNullOrEmpty(apiTokenOptions.IdentityServerBaseUrl))
                {
                    var securityName = "oauth2";
                    options.AddSecurityDefinition(securityName, new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri($"{apiTokenOptions.IdentityServerBaseUrl}/connect/authorize"),
                                TokenUrl = new Uri($"{apiTokenOptions.IdentityServerBaseUrl}/connect/token"),
                                Scopes = new Dictionary<string, string>
                                {
                                    {apiTokenOptions.OidcApiName, apiTokenOptions.ApiName}
                                }
                            }
                        }
                    });
                    //options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    //{
                    //    {
                    //        new OpenApiSecurityScheme
                    //        {
                    //            Reference = new OpenApiReference
                    //            {
                    //                Type = ReferenceType.SecurityScheme,
                    //                Id = securityName,
                    //            },
                    //            Scheme =securityName,
                    //            Name = securityName,
                    //            In = ParameterLocation.Header,
                    //        }, new List<string>()
                    //    },
                    //});
                    options.OperationFilter<AuthorizeCheckOperationFilter>();
                }

                // using System.Reflection;
                var xmlFilename = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerUIConfig(this IApplicationBuilder app, ApiTokenOptions apiTokenOptions)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{apiTokenOptions.ApiBaseUrl}/swagger/v1/swagger.json", apiTokenOptions.ApiName);
                if (!string.IsNullOrEmpty(apiTokenOptions.OidcSwaggerUIClientId))
                {
                    c.OAuthClientId(apiTokenOptions.OidcSwaggerUIClientId);
                }
                c.OAuthAppName(apiTokenOptions.ApiName);
                c.OAuthUsePkce();
                c.RoutePrefix = string.Empty;
            });

            return app;
        }
    }
}