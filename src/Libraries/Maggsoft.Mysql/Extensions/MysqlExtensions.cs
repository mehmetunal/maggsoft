using System;
using FluentMigrator;
using FluentMigrator.Runner;
using Maggsoft.Data.DataProviders;
using Maggsoft.Data.Migration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using MySqlConnector;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace Maggsoft.Mysql.Extensions;

public static class MysqlExtensions
{
    public static IServiceCollection AddMysqlConfig<TContext>(this IServiceCollection services,
        IConfiguration configuration, Action<MySqlDbContextOptionsBuilder>? mySqlOptionsAction = null) where TContext : DbContext
    {
        var connection = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<TContext>(options =>
        {
            options.UseMySql(connection, ServerVersion.AutoDetect(connection), mySqlOptionsAction);
        });
        services.AddScoped<DbContext, TContext>();
        return services;
    }
    
    public static IServiceCollection AddMysqlConfig<TContext>(this IServiceCollection services,
        string connectionString, Action<MySqlDbContextOptionsBuilder>? mySqlOptionsAction = null) where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptionsAction);
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
                builder.AddMySql5();
                builder.WithGlobalConnectionString(connection);
                builder.ScanIn(mAssemblies).For.Migrations();
            })
            .AddLogging(op => op.AddFluentMigratorConsole());

        services.AddTransient(p => new Lazy<IVersionLoader>(p.GetRequiredService<IVersionLoader>()));
        services.AddScoped<IMigrationManager, MigrationManager>();

        return services;
    }
    public static IServiceCollection AddFluentMigratorConfig(this IServiceCollection services,
        string connectionString) 
    {
        var mAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(MigrationBase).IsAssignableFrom(p) && p.IsClass == true)
            .Select(t => t.Assembly)
            .Where(assembly => !assembly.FullName.Contains("FluentMigrator.Runner"))
            .Distinct()
            .ToArray();

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(builder =>
            {
                builder.AddMySql5();
                builder.WithGlobalConnectionString(connectionString);
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

        var databaseName = builder.Database;
        builder.Database = string.Empty;

        using (var connection = GetInternalDbConnection(builder.ConnectionString))
        {
            var query = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`";

            var command = connection.CreateCommand();
            command.CommandText = query;
            command.Connection.Open();

            command.ExecuteNonQuery();
        }

        //try connect
        if (triesToConnect <= 0)
            return host;
 
        for (var i = 0; i <= triesToConnect; i++)
        {
            if (i == triesToConnect)
                throw new Exception("Unable to connect to the new database. Please try one more time");

            if (!DatabaseExists(host))
                Thread.Sleep(1000);
            else
                break;
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
            using var context = GetInternalDbConnection(DataProviderExtensions.GetCurrentConnectionString(host));
            context.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static MySqlConnectionStringBuilder GetConnectionStringBuilder(IHost host)
    {
        return new MySqlConnectionStringBuilder(DataProviderExtensions.GetCurrentConnectionString(host));
    }

    private static DbConnection GetInternalDbConnection(string connectionString)
    {
        return new MySqlConnection(connectionString);
    }
}