using Dev.Framework.Exceptions;
using Dev.Framework.Security.Model;
using Dev.Framework.Security.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dev.Framework.Extensions
{
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
                            context.Response.Headers.Add("Token-Expired", "true");
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
                            context.Request.Headers.TryGetValue("Authorization", out var BearerToken);
                            if (BearerToken.Count == 0 && tokenOptions.IgnoreUrls.Any(p => p == context.Request.Path) == false)
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
                });
            return services;
        }
    }
}