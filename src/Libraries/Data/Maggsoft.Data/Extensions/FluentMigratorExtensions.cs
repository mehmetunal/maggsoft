using FluentMigrator;
using FluentMigrator.Runner;
using Maggsoft.Data.Migration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Maggsoft.Data.Extensions
{
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

            var mAssemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p => typeof(MigrationBase).IsAssignableFrom(p) && p.IsClass == true).Select(t => t.Assembly)
                .Where(assembly => !assembly.FullName.Contains("FluentMigrator.Runner")).Distinct().ToArray();

            foreach (var assembly in mAssemblies)
            {
                call(runner, assembly);
            }

            return app;
        }
    }
}
