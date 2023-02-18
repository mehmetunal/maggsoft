using Maggsoft.EventBus.IoC;
using Maggsoft.EventBus.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Maggsoft.EventBus.Example.Subscribe.IntegrationEvents.Events;
using Maggsoft.EventBus.Example.Subscribe.IntegrationEvents.EventHandling;

namespace Maggsoft.EventBus.Example.Subscribe
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

            DependencyContainer.RegisterEventBusConntionServices(services, Configuration);
            DependencyContainer.RegisterEventBus(services, Configuration);

            services.AddTransient<IIntegrationEventHandler<MainChangedIntegrationEvent>, MainChangedIntegrationEventHandler>();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddOptions();
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            ConfigureEventBus(app);
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<MainChangedIntegrationEvent, IIntegrationEventHandler<MainChangedIntegrationEvent>>();
        }
    }
}
