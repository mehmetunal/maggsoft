using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplicationExample.Service;

namespace WebApplicationExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ITestService _testService;

        public WeatherForecastController(ITestService testService)
            => _testService = testService;

        //[HttpGet]
        //public string Get()
        //    => _testService.Get();

        //[HttpGet("remove")]
        //public string Remove()
        //    => _testService.Remove();



        [HttpGet]
        public async Task<List<WeatherForecast>> Get()
            => await _testService.Get();

        [HttpGet("remove")]
        public Task<List<WeatherForecast>> Remove()
            => _testService.Remove();
    }
}
