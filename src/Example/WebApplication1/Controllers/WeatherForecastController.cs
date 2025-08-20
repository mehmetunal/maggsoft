using Maggsoft.Core.Base;
using Maggsoft.Core.Model;
using Maggsoft.Framework.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;
using Maggsoft.Core.Infrastructure;

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
            var asd = MaggsoftContext.Current.Resolve<ILogger<WeatherForecastController>>();
            _logger = logger;
        }

        [HttpGet]
        public Result<List<WeatherForecast>> Get()
        {
            var model = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToList();

            //return Result.Success();
            // return Result<IReadOnlyList<WeatherForecast>>.Success(model,StatusCodes.Status226IMUsed, FollowerMessage.OK);
            //return Result<IList<WeatherForecast>>.Success(model,SuccessMessage.None);
            //return Result<IList<WeatherForecast>>.Success(model,FollowerMessage.OK);

            return model;
        }

        [HttpGet]
        [Route("mesaj")]
        public void Mesaj() => throw new ArgumentNullException($"{nameof(Get)} is not null");

        [HttpGet]
        [Route("smesaj")]
        public void SMesaj() => throw new Exception($"{nameof(Get)} is not null");

        [HttpGet]
        [Route("vmesaj")]
        public void VMesaj() => throw new ModelStateException($"{nameof(Get)} is not null");

        [HttpGet]
        [Route("emesaj")]
        public Result EMesaj() => Result.Failure(["sadasd"]);
    }

}