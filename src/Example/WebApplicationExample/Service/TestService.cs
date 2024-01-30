using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Maggsoft.Cache;

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

        public async Task<List<WeatherForecast>> Get()
        {
            var result = await _cache.GetAsync(cacheKey, TimeSpan.FromSeconds(15), async () =>
            {
                var data = await GetAll();
                return data;
            });

            return result;
        }

        public Task<List<WeatherForecast>> Remove()
        {
            _cache.Remove(cacheKey);

            var result = _cache.GetAsync(cacheKey, TimeSpan.FromSeconds(15), async () =>
            {
                var data = await GetAll();
                return data;
            });
            return result;
        }

        private static async Task<List<WeatherForecast>> GetAll()
        {
            var data = Enumerable.Range(0, 1200)
                     .Select(index => new WeatherForecast
                     {
                         Date = DateTime.Now.AddDays(index),
                         Summary = $"{index} - Mehnme ÜNAL",
                         TemperatureC = index
                     }).ToList();

            return await Task.FromResult(data);
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
