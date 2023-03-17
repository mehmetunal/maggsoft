using Maggsoft.Cache.Attributes;
using Maggsoft.Cache.Helpers;
using Maggsoft.Core.IoC;
using Maggsoft.Logging.Aspect;

namespace WebApplicationExample.Service
{
    public interface ITestService : IService
    {
        [AOPLogging]
        [Cache(TTL = 30 * TTLMultiplier.Day, SlidingExpiration = false)]
        string Get();

        [AOPLogging]
        [CacheInvalidate]
        string Remove();
    }
}