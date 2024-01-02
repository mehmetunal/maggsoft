using Maggsoft.Core.IoC;
using Maggsoft.Data.Extensions;
using Maggsoft.ExampleTest.Entity;
using Maggsoft.Mssql.Extensions;
using Maggsoft.Mssql.Repository;
using Maggsoft.Mssql.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AppContext = Maggsoft.ExampleTest.Context.AppContext;

namespace Maggsoft.ExampleTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services
                .AddMssqlConfig<AppContext>(Configuration)
                .AddFluentMigratorConfig(Configuration);

            //services.AddScoped<INpgsqlRepository<User>,NpgsqlRepository<User>>();
            //services.AddScoped<IUnitOfWork,UnitOfWork>();



            services.AddScoped<IMssqlRepository<User>, Repository<User>>();
            services.AddScoped<IMssqlRepository<Log>, Repository<Log>>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.RegisterAll<IService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.AddUpMigrate();
            //app.AddDownMigrate();
        }
    }
}
