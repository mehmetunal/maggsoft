using FluentMigrator;
using Maggsoft.Data.Migration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Maggsoft.Data.Extensions;

public static class FluentMigratorExtensions
{
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

        var currentDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var currentDomainAssembliesTypes = currentDomainAssemblies.SelectMany(s => s.GetTypes());
        
        var migrationBaseAssembly = currentDomainAssembliesTypes
            .Where(p => p.IsClass == true && p.IsAbstract == false &&typeof(MigrationBase).IsAssignableFrom(p))
            .Select(t => t.Assembly);


        var mAssemblies = migrationBaseAssembly.Where(assembly => !assembly.FullName.Contains("FluentMigrator.Runner")).Distinct().ToArray();

        foreach (var assembly in mAssemblies)
        {
            call(runner, assembly);
        }

        return app;
    }
}