using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using WebApplicationExample.Service;

namespace WebApplicationExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ITestService _testService;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ITestService testService)
        {
            _logger = logger;
            _testService = testService;
        }

        [HttpGet]
        public string Get()
            => _testService.Get();

        [HttpGet("remove")]
        public string remove()
        {
            var a = GetCache<object>();
            return _testService.Remove();
        }


        public virtual List<T> GetCache<T>()
        {
            ObjectCache cache = MemoryCache.Default;
            // Store data in the cache    
            //CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
            //cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddHours(1.0);
            //cache.Add("tt", "as", cacheItemPolicy);

            //cache = MemoryCache.Default;
            // Define array for 1 cache host
            List<T> list = new List<T>();
            //foreach (var item in cache)
            //{
            //    //add the item.keys to list
            //}
         
            return list;
        }
    }
}
