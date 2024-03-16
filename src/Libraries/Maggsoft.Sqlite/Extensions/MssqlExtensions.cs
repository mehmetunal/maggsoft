using FluentMigrator;
using FluentMigrator.Runner;
using Maggsoft.Data.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Maggsoft.Sqlite.Extensions;

public static class SqliteExtensions
{
    public static IServiceCollection AddSqliteConfig<TContext>(this IServiceCollection services,
        IConfiguration configuration) where TContext : DbContext
    {
        var connection = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<TContext>(options => { options.UseSqlite(connection); });
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
                builder.AddSQLite();
                builder.WithGlobalConnectionString(connection);
                builder.ScanIn(mAssemblies).For.Migrations();
            })
            .AddLogging(op => op.AddFluentMigratorConsole());

        services.AddTransient(p => new Lazy<IVersionLoader>(p.GetRequiredService<IVersionLoader>()));
        services.AddScoped<IMigrationManager, MigrationManager>();

        return services;
    }
}