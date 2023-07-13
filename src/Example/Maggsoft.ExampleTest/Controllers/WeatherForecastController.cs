using Maggsoft.ExampleTest.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maggsoft.ExampleTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly Context.NpgsqlContext _npgsqlContext;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, Context.NpgsqlContext npgsqlContext)
        {
            _logger = logger;
            _npgsqlContext = npgsqlContext;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var user = new Entity.User { Text = "t" };
            user.Logs.Add(new Log() { Text = "tt", UserId = user.Id });

            _npgsqlContext.Users.Add(user);
            _npgsqlContext.SaveChanges();


            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
