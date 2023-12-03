using Maggsoft.Framework.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
            => Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        [HttpGet]
        [Route("mesaj")]
        public void Mesaj() => throw new ArgumentNullException($"{nameof(Get)} is not null");

        [HttpGet]
        [Route("smesaj")]
        public void SMesaj() => throw new Exception($"{nameof(Get)} is not null");

        [HttpGet]
        [Route("vmesaj")]
        public void VMesaj() => throw new ModelStateException($"{nameof(Get)} is not null");
    }
}