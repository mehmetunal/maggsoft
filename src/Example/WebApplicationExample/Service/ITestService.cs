using Maggsoft.Cache.Attributes;
using Maggsoft.Cache.Helpers;
using Maggsoft.Core.IoC;
using Maggsoft.Logging.Aspect;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplicationExample.Service
{
    public interface ITestService : IService
    {
        /* [AOPLogging]
         [Cache(TTL = 30 * TTLMultiplier.Day, SlidingExpiration = false)]
         string Get();

         [AOPLogging]
         [CacheInvalidate]
         string Remove();*/

        Task<List<WeatherForecast>> Get();


        Task<List<WeatherForecast>> Remove();
    }
}