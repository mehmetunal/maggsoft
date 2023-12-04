using Maggsoft.Core.Base;
using Maggsoft.Core.Model;
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

        [HttpGet]
        [Route("emesaj")]
        public Result EMesaj() => Result.Failure(FollowerErrors.SameUser);
    }
    public static class FollowerErrors
    {
        public static Error NotFound(Guid id) => new Error(
       "Followers.NotFound", $"The follower with Id '{id}' was not found");

        public static readonly Error SameUser = new Error(
            "Followers.SameUser", "Can't follow yourself");

        public static readonly Error NonPublicProfile = new Error(
            "Followers.NonPublicProfile", "Can't follow non-public profiles");

        public static readonly Error AlreadyFollowing = new Error(
            "Followers.AlreadyFollowing", "Already following");
    }
}