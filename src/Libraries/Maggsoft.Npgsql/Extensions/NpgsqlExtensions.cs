using FluentMigrator;
using FluentMigrator.Runner;
using Maggsoft.Core.IoC;
using Maggsoft.Data.Migration;
using Maggsoft.Npgsql.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;

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
                ).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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

        /// <summary>  
        /// Migrates the database.  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="host">The web host.</param>  
        /// <returns>IWebHost.</returns>  
        public static IHost CreateDatabase(this IHost host, int triesToConnect = 10)
        {
            if (DatabaseExists(host))
                return host;

            var builder = GetConnectionStringBuilder(host);

            //gets database name
            var databaseName = builder.Database;

            //now create connection string to 'postgres' - default administrative connection database.
            builder.Database = "postgres";

            using (var connection = GetInternalDbConnection(builder.ConnectionString))
            {
                var query = $"CREATE DATABASE \"{databaseName}\" WITH OWNER = '{builder.Username}'";

                var command = connection.CreateCommand();
                command.CommandText = query;
                command.Connection.Open();

                command.ExecuteNonQuery();
            }

            //try connect
            if (triesToConnect <= 0)
                return host;

            //sometimes on slow servers (hosting) there could be situations when database requires some time to be created.
            //but we have already started creation of tables and sample data.
            //as a result there is an exception thrown and the installation process cannot continue.
            //that's why we are in a cycle of "triesToConnect" times trying to connect to a database with a delay of one second.
            for (var i = 0; i <= triesToConnect; i++)
            {
                if (i == triesToConnect)
                    throw new Exception("Unable to connect to the new database. Please try one more time");

                if (!DatabaseExists(host))
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    builder.Database = databaseName;
                    using var connection = GetInternalDbConnection(builder.ConnectionString) as NpgsqlConnection;
                    var command = connection.CreateCommand();
                    command.CommandText = "CREATE EXTENSION IF NOT EXISTS citext; CREATE EXTENSION IF NOT EXISTS pgcrypto; CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";";
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    connection.ReloadTypes();

                    break;
                }
            }

            return host;
        }
        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="host">IHost</param>
        /// <returns>Returns true if the database exists.</returns>
        private static bool DatabaseExists(IHost host)
        {
            try
            {
                using var context = GetInternalDbConnection(GetCurrentConnectionString(host));
                context.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static NpgsqlConnectionStringBuilder GetConnectionStringBuilder(IHost host)
        {
            return new NpgsqlConnectionStringBuilder(GetCurrentConnectionString(host));
        }

        private static DbConnection GetInternalDbConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        private static string GetCurrentConnectionString(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            return configuration.GetConnectionString("DefaultConnection");
        }
    }
}
