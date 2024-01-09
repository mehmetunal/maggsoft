using Maggsoft.Cache;
using System;

namespace WebApplicationExample.Service
{
    public class TestService : ITestService
    {
        private readonly ICache _cache;
        private readonly string cacheKey = nameof(TestService.Get);
        public TestService(ICache cache)
        {
            _cache = cache;
        }

        public WeatherForecast Get()
        {
            var result = _cache.Get(cacheKey, TimeSpan.FromSeconds(15), () =>
            {
                return new WeatherForecast() { Date = DateTime.Now, Summary = "mehmet", TemperatureC = 5 };
            });

            return result;
        }

        public WeatherForecast Remove()
        {
            _cache.Remove(cacheKey);
            var result = _cache.Get(cacheKey, TimeSpan.FromSeconds(15), () =>
            {
                return new WeatherForecast() { Date = DateTime.Now, Summary = "mehmet___2", TemperatureC = 5 };
            });
            return result;
        }
        /*
public string Get()
{
   var result = _cache.Get<string>(cacheKey, TimeSpan.FromSeconds(15), () =>
   {
       return "mehmet";
   });

   return result;
   /*var cache = _cache.Get<string>(nameof(TestService.Get));
   if (!string.IsNullOrEmpty(cache)) return cache;
   _cache.Set(nameof(TestService.Get), TimeSpan.FromTicks(DateTime.Now.AddDays(2).Ticks), true, "mehmet");
   cache = _cache.Get<string>(nameof(TestService.Get));
   return cache;*/
        /*}*/
        /*
            public string Remove()
            {
                _cache.Remove(cacheKey);
                var result = _cache.Get<string>(cacheKey, TimeSpan.FromSeconds(15), () =>
                {
                    return "mehmet___2";
                });
                return result;
            }*/
    }
}
