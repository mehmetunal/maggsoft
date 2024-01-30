using Maggsoft.ExampleTest.Services;
using Maggsoft.Mssql.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Maggsoft.ExampleTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(ILogger<WeatherForecastController> logger, Context.AppContext npgsqlContext, IUnitOfWork uow, IUserService userService) : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly Context.AppContext _npgsqlContext = npgsqlContext;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<WeatherForecastController> _logger = logger;

        private readonly IUserService userService = userService;

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            _logger.LogWarning("Örnek Login");
            _logger.LogError("Örnek Login");
            try
            {
                throw new NotImplementedException();
            }
            catch (NotImplementedException ex)
            {
                _logger.LogError("Serilog Error Test", ex.Message, ex);
                throw;
            }
            return Ok(await userService.AddAsync(new Dto.UserAddDto { Text = "asdasdasdasd" }));

            //var asd = await this.userService.GetAsync(0, int.MaxValue, true, includes: p => p.Include(o => o.Logs.OrderByDescending(od => od.Text)));

            //return Ok(asd);
            //var users = _npgsqlContext.Users.ToList();
            //var user = new Entity.User { Text = "tt", CreatorIP = "123", CreatedDate = DateTime.UtcNow, CreatorUserId = Guid.NewGuid() };
            //user.Logs.Add(new Log() { Text = "MENU 1", UserId = user.Id, CreatorIP = "123", CreatedDate = DateTime.UtcNow, CreatorUserId = Guid.NewGuid() });
            //user.Logs.Add(new Log() { Text = "MENU 2", UserId = user.Id, CreatorIP = "123", CreatedDate = DateTime.UtcNow, CreatorUserId = Guid.NewGuid() });
            //user.Logs.Add(new Log() { Text = "MENU 3", UserId = user.Id, CreatorIP = "123", CreatedDate = DateTime.UtcNow, CreatorUserId = Guid.NewGuid() });
            //user.Logs.Add(new Log() { Text = "MENU 4", UserId = user.Id, CreatorIP = "123", CreatedDate = DateTime.UtcNow, CreatorUserId = Guid.NewGuid() });
            //_npgsqlContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            //_npgsqlContext.Users.Add(user);

            ////var userDb = _npgsqlContext.Users.FirstOrDefault(p => p.Id == 1);
            ////userDb.Text = "t3";
            //var entries = _npgsqlContext.ChangeTracker.Entries();

            //foreach (var entry in entries)
            //{
            //    //Console.WriteLine($"Entity: {entry.Entity.GetType().Name},


            //    //                     State: { entry.State.ToString()}
            //    //");
            //}
            //_npgsqlContext.SaveChanges();
            //var user = new Entity.User
            //{
            //    Text = "Memoliasdasdasdsdfsdfsdfsdf",
            //    CreatedDate = DateTime.Now,
            //    CreatorIP = "123.1",
            //    CreatorUserId = Guid.NewGuid(),
            //};
            //user.Logs.Add(new Entity.Log { Text = "Memoli", CreatedDate = DateTime.Now, CreatorIP = "123.1", CreatorUserId = Guid.NewGuid() });

            //using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            //{
            //    try
            //    {
            //        _npgsqlContext.Add(user);
            //        _npgsqlContext.SaveChanges();
            //        scope.Dispose();
            //    }
            //    catch (Exception)
            //    {
            //        scope.Dispose();
            //        throw;
            //    }
            //}

            //var beginTran = _uow.BeginNewTransaction();
            //_uow.GetRepository<User>().Add(user);
            ////var log = new Entity.Log { UserId = user.Id, Text = "Memoli", CreatedDate = DateTime.Now, CreatorIP = "123.1", CreatorUserId = Guid.NewGuid() };
            ////_uow.GetRepository<Log>().Add(log);
            //_uow.SaveChanges();

            //var rng = new Random();
            //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateTime.Now.AddDays(index),
            //    TemperatureC = rng.Next(-20, 55),
            //    Summary = Summaries[rng.Next(Summaries.Length)]
            //})
            //.ToArray();
        }
    }
}