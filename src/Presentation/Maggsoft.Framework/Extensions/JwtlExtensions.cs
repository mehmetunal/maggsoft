using Maggsoft.Framework.Exceptions;
using Maggsoft.Framework.Security.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtConfig(this IServiceCollection services,
        IConfiguration configuration)
    {
        var tokenOptionsConfiguration = configuration.GetSection("TokenOptions");

        services.Configure<ApiTokenOptions>(tokenOptionsConfiguration);

        var tokenOptions = tokenOptionsConfiguration.Get<ApiTokenOptions>();

        services.AddAuthentication(x =>
            {
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Authority = tokenOptions.IdentityServerBaseUrl;
                x.RequireHttpsMetadata = tokenOptions.RequireHttpsMetadata;
                x.Audience = tokenOptions.OidcApiName;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    //Gelen isteğin doğru siteden olduğunu kontrol eder,
                    ValidateAudience = true,
                    // Validet eilecek Issure
                    ValidIssuer = tokenOptions.IdentityServerBaseUrl,
                    //Gelen isteğin doğru siteden olduğunu kontrol eder, //Bu iki ayar ise "aud" ve "iss" claimlerini kontrol edelim mi diye soruyor
                    ValidateIssuer = false,
                    //Gelen her tokenun doğrulankasını sağlıyor.Token 3.kısım(imza) kontrolü
                    ValidateIssuerSigningKey = true,
                    //Doğrulama Keyini Tanımladığımız yer.Neyle kontrol etmesi gerektigi
                    //IssuerSigningKey = SingHandler.GetSecurityKey(tokenOptions.SecurityKey),

                    ClockSkew = TimeSpan.Zero,
                    //Süresi dolmamış token vermesini sağlıyor
                    ValidateLifetime = true,
                };
                x.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = (context) =>
                    {
                        context.NoResult();
                        context.Response.Headers.TryAdd("Token-Expired", "true");
                        throw new UnauthorizedAccessException();
                        return Task.CompletedTask;
                    },
                    OnForbidden = (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        throw new ForbiddenExtension("Forbidden");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = (context) =>
                    {
                        if (tokenOptions.IgnoreUrls.Any(p => context.Request.Path.HasValue && context.Request.Path.Value.StartsWith(p)) == true)
                            return Task.CompletedTask;

                        context.Request.Headers.TryGetValue("Authorization", out var BearerToken);
                        if (BearerToken.Count == 0)
                            throw new UnauthorizedAccessException();

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = (context) => { return Task.CompletedTask; },
                    OnChallenge = (context) =>
                    {
                        context.HandleResponse();
                        throw new UnauthorizedAccessException();
                        return Task.CompletedTask;
                    }
                };
                x.BackchannelHttpHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                /*
                  x.JwtBackChannelHandler = new HttpClientHandler
                {
                    DefaultProxyCredentials = CredentialCache.DefaultCredentials
                };

                  x.BackchannelHttpHandler = new HttpClientHandler
            {
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };
                 https://gitter.im/IdentityServer/IdentityServer4?at=5d386c06437a3a13484950aa
                https://support.abp.io/QA/Questions/2659/Identity-API-with-JWT
                https://support.abp.io/QA/Questions/491/Mac-devlop-problem-error-IDX20803
                https://blog.antosubash.com/posts/abp-deploy-with-docker

                https://www.appsloveworld.com/csharp/100/605/building-an-integration-test-for-an-aspnetcore-api-that-uses-identityserver-4-for
                 */
            });
        return services;
    }
    public static IServiceCollection AddOcelotJwtConfig(this IServiceCollection services,
        IConfiguration configuration)
    {
        var tokenOptionsConfiguration = configuration.GetSection("TokenOptions");

        services.Configure<ApiTokenOptions>(tokenOptionsConfiguration);

        var tokenOptions = tokenOptionsConfiguration.Get<ApiTokenOptions>();

        services.AddAuthentication(x =>
        {
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
            {
                x.Authority = tokenOptions.IdentityServerBaseUrl;
                x.RequireHttpsMetadata = tokenOptions.RequireHttpsMetadata;
                x.Audience = tokenOptions.OidcApiName;
                x.SaveToken = true;
                x.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = (context) =>
                    {
                        context.NoResult();
                        context.Response.Headers.TryAdd("Token-Expired", "true");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnForbidden = (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = (context) =>
                    {
                        if (tokenOptions.IgnoreUrls.Any(p => context.Request.Path.HasValue && context.Request.Path.Value.StartsWith(p)) == true)
                            return Task.CompletedTask;

                        context.Request.Headers.TryGetValue("Authorization", out var BearerToken);
                        if (BearerToken.Count == 0)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = (context) => { return Task.CompletedTask; },
                    OnChallenge = (context) =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                };
                x.BackchannelHttpHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                /*
                  x.JwtBackChannelHandler = new HttpClientHandler
                {
                    DefaultProxyCredentials = CredentialCache.DefaultCredentials
                };

                  x.BackchannelHttpHandler = new HttpClientHandler
            {
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };
                 https://gitter.im/IdentityServer/IdentityServer4?at=5d386c06437a3a13484950aa
                https://support.abp.io/QA/Questions/2659/Identity-API-with-JWT
                https://support.abp.io/QA/Questions/491/Mac-devlop-problem-error-IDX20803
                https://blog.antosubash.com/posts/abp-deploy-with-docker

                https://www.appsloveworld.com/csharp/100/605/building-an-integration-test-for-an-aspnetcore-api-that-uses-identityserver-4-for
                 */
            });
        return services;
    }
}