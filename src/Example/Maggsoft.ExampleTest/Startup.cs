using FluentMigrator.Runner;
using Maggsoft.ExampleTest.Context;
using Maggsoft.ExampleTest.Services;
using Maggsoft.Npgsql.Extensions;
using Maggsoft.Npgsql.Repository;
using Maggsoft.Npgsql.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Maggsoft.Services.Extensions;
using Maggsoft.Framework.Extensions;
using Maggsoft.ExampleTest.Entity;
using Maggsoft.Core.IoC;

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
            services.AddHttpContextAccessor();
            services
                .AddNpgsqlConfig<NpgsqlContext>(Configuration)
                .AddFluentMigratorConfig(Configuration);

            services.AddAutoMapperConfig(p => p.AddProfile<AutoMapping>(), typeof(Startup));

            services.AddScoped<INpgsqlRepository<User>,NpgsqlRepository<User>>();
            services.AddScoped<IUnitOfWork,UnitOfWork>();
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
            app.ConfigureRequestPipeline();
        }
    }
}
