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
        public Result Get()
        {
            var model = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToList();

            //return Result.Success();
            //return Result.Success(FollowerMessage.OK);

            //return Result<IList<WeatherForecast>>.Success(model,SuccessMessage.None);
            return Result<IList<WeatherForecast>>.Success(model,FollowerMessage.OK);

            //return model;
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
        public Result EMesaj() => Result.Failure(FollowerErrors.SameUser);
    }
    public static class FollowerErrors
    {
        public static Error NotFound(Guid id) => new("Followers.NotFound", $"The follower with Id '{id}' was not found");

        public static readonly Error SameUser = new ("Followers.SameUser", "Can't follow yourself");

        public static readonly Error NonPublicProfile = new("Followers.NonPublicProfile", "Can't follow non-public profiles");

        public static readonly Error AlreadyFollowing = new("Followers.AlreadyFollowing", "Already following");
    }

    public static class FollowerMessage
    {
        public static readonly SuccessMessage OK = new("Ok", "İşlem Başarılı");
    }
}