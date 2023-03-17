using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Maggsoft.Core.Infrastructure;
using Maggsoft.Data.Migration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maggsoft.Npgsql.Extensions
{
    public static class NpgsqlExtensions
    {
        public static IServiceCollection AddNpgsqlConfig<TContext>(this IServiceCollection services,
            IConfiguration configuration) where TContext : DbContext
        {
            var connection = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TContext>(options =>
            {
                options.UseNpgsql(connection,
                    conOption => conOption.EnableRetryOnFailure(
                            maxRetryCount: 15,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null
                        )
                );
            });
            services.AddScoped<DbContext, TContext>();
            return services;
        }
        public static IServiceCollection AddFluentMigratorConfig(this IServiceCollection services,
            IConfiguration configuration)
        {
            var mAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(MigrationBase).IsAssignableFrom(p) && p.IsClass == true)
                .Select(t => t.Assembly)
                .Where(assembly => !assembly.FullName.Contains("FluentMigrator.Runner"))
                .Distinct()
                .ToArray();

            var connection = configuration.GetConnectionString("DefaultConnection");
            services
                .AddFluentMigratorCore()
                .ConfigureRunner(builder =>
                {
                    builder.AddPostgres11_0();
                    builder.WithGlobalConnectionString(connection);
                    builder.ScanIn(mAssemblies).For.Migrations();
                })
                .AddLogging(op => op.AddFluentMigratorConsole());

            services.AddTransient(p => new Lazy<IVersionLoader>(p.GetRequiredService<IVersionLoader>()));
            services.AddScoped<IMigrationManager, MigrationManager>();

            return services;
        }

        // <summary>
        // 
        // </summary>
        // <param name = "app" ></ param >
        // < typeparam name="TContext"></typeparam>
        // <returns></returns>
        public static IApplicationBuilder AddMigrateConfigure<TContext>(this IApplicationBuilder app) where TContext : DbContext
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();
                context.Database.EnsureCreated();
                //context.Database.Migrate();
            }

            return app;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder AddUpMigrate(this IApplicationBuilder app)
        {
            return RunMigration(app, (runner, assembly) => runner.ApplyUpMigrations(assembly));
        }


        public static IApplicationBuilder AddDownMigrate(this IApplicationBuilder app)
        {
            return RunMigration(app, (runner, assembly) => runner.ApplyDownMigrations(assembly));
        }

        private static IApplicationBuilder RunMigration(IApplicationBuilder app, Action<IMigrationManager, Assembly> call)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationManager>();

            var mAssemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p => typeof(MigrationBase).IsAssignableFrom(p) && p.IsClass == true).Select(t => t.Assembly)
                .Where(assembly => !assembly.FullName.Contains("FluentMigrator.Runner")).Distinct().ToArray();

            foreach (var assembly in mAssemblies)
            {
                call(runner, assembly);
            }

            return app;
        }

        /// <summary>  
        /// Migrates the database.  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="host">The web host.</param>  
        /// <returns>IWebHost.</returns>  
        public static IHost CreateDatabase<TContext>(this IHost host) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            try
            {
                var context = services.GetRequiredService<TContext>();
                RelationalDatabaseCreator databaseCreator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
                if (!databaseCreator.Exists())
                {
                    bool ensureCreated = context.Database.EnsureCreated();
                    //databaseCreator.CreateTables();
                    //context.Database.Migrate();
                }
                Console.WriteLine("Database migration completed.");
                logger.LogInformation("Database migration completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred migrate the DB.");
            }

            return host;
        }
    }
}
