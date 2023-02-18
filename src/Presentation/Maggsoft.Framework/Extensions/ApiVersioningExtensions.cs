using Maggsoft.Framework.Helper.ApiVersioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maggsoft.Framework.Extensions
{
    public static class ApiVersioningExtensions
    {
        /// <summary>
        /// AddApiVersioningConfig
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddApiVersioningConfig(this IServiceCollection services,
            IConfiguration configuration)
        {
            var majorVersion = 1;
            var minorVersion = 1;

            var majorVersionConfig = configuration.GetSection("ApiVersion:MajorVersion")?.Value;
            var minorVersionConfig = configuration.GetSection("ApiVersion:MinorVersion")?.Value;

            if (!string.IsNullOrEmpty(majorVersionConfig))
                majorVersion = int.Parse(majorVersionConfig);

            if (!string.IsNullOrEmpty(minorVersionConfig))
                minorVersion = int.Parse(minorVersionConfig);

            services.AddApiVersioning(option =>
            {
                // Varsayılan API Sürümü
                option.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);

                // İstemci istekte API sürümünü belirtmediyse, varsayılan API sürüm numarası kullanma 
                option.AssumeDefaultVersionWhenUnspecified = true;

                // Belirli bir son nokta için desteklenen API sürümlerinin bilgisini verme
                option.ReportApiVersions = true;

                //Api Versioing ile ilgili exceptionlerı result değiştirme
                option.ErrorResponses = new ApiVersioningErrorResponseProvider();

                // İstemci X sürümü üstbilgisini kullanarak belirli sürümü ister
                // option.ApiVersionReader = ApiVersionReader.Combine(new HeaderApiVersionReader("x-api-version"),
                //     new QueryStringApiVersionReader("api-version"));
            });
            return services;
        }
    }
}