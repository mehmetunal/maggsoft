using FluentMigrator;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Maggsoft.Data.Migration.Attribute;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Maggsoft.Data.Migration
{
    /// <summary>
    /// Represents the migration manager
    /// </summary>
    public partial class MigrationManager : IMigrationManager
    {
        #region Fields

        private readonly IFilteringMigrationSource _filteringMigrationSource;
        private readonly IMigrationRunner _migrationRunner;
        private readonly IMigrationRunnerConventions _migrationRunnerConventions;
        private readonly Lazy<IVersionLoader> _versionLoader;

        #endregion

        #region Ctor

        public MigrationManager(
            IFilteringMigrationSource filteringMigrationSource,
            IMigrationRunner migrationRunner,
            IMigrationRunnerConventions migrationRunnerConventions,
            Lazy<IVersionLoader> versionLoader)
        {
            _versionLoader = versionLoader;

            _filteringMigrationSource = filteringMigrationSource;
            _migrationRunner = migrationRunner;
            _migrationRunnerConventions = migrationRunnerConventions;
        }

        #endregion
        #region Utils

        /// <summary>
        /// Returns the instances for found types implementing FluentMigrator.IMigration which ready to Up process
        /// </summary>
        /// <param name="assembly">Assembly to find migrations</param>
        /// <returns>The instances for found types implementing FluentMigrator.IMigration</returns>
        protected virtual IEnumerable<IMigrationInfo> GetUpMigrations(Assembly assembly)
        {
            var migrations = _filteringMigrationSource
                .GetMigrations(t =>
                {
                    var migrationAttribute = t.GetCustomAttribute<MaggsoftMigrationAttribute>();

                    if (migrationAttribute is null || _versionLoader.Value.VersionInfo.HasAppliedMigration(migrationAttribute.Version))
                        return false;

                    return assembly == null || t.Assembly == assembly;

                }) ?? Enumerable.Empty<IMigration>();

            return migrations
                .Select(m => _migrationRunnerConventions.GetMigrationInfoForMigration(m))
                .OrderBy(migration => migration.Version);
        }

        /// <summary>
        /// Returns the instances for found types implementing FluentMigrator.IMigration which ready to Down process
        /// </summary>
        /// <param name="assembly">Assembly to find migrations</param>
        /// <returns>The instances for found types implementing FluentMigrator.IMigration</returns>
        protected virtual IEnumerable<IMigrationInfo> GetDownMigrations(Assembly assembly)
        {
            var migrations = _filteringMigrationSource
                .GetMigrations(t =>
                {
                    var migrationAttribute = t.GetCustomAttribute<MaggsoftMigrationAttribute>();

                    if (migrationAttribute is null || !_versionLoader.Value.VersionInfo.HasAppliedMigration(migrationAttribute.Version))
                        return false;

                    return assembly == null || t.Assembly == assembly;
                }) ?? Enumerable.Empty<IMigration>();

            return migrations
                .Select(m => _migrationRunnerConventions.GetMigrationInfoForMigration(m))
                .OrderBy(migration => migration.Version);
        }

        #endregion


        #region Methods

        /// <summary>
        /// Executes an Up for all found unapplied migrations
        /// </summary>
        /// <param name="assembly">Assembly to find migrations</param>
        /// <param name="commitVersionOnly">Commit only version information</param>
        public virtual void ApplyUpMigrations(Assembly assembly, bool commitVersionOnly = false)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            foreach (var migrationInfo in GetUpMigrations(assembly))
            {
                if (!commitVersionOnly)
                    _migrationRunner.Up(migrationInfo.Migration);

                try
                {
                    _versionLoader.Value
                        .UpdateVersionInfo(migrationInfo.Version,
                            migrationInfo.Description ?? migrationInfo.Migration.GetType().Name);
                }
                catch
                {
                    //TODO: refactoring the GetUpMigrations to get directly selected MigrationProcessType for commitVersionOnly == true
                }
            }
        }

        /// <summary>
        /// Executes a Down for all found (and applied) migrations
        /// </summary>
        /// <param name="assembly">Assembly to find the migration</param>
        public void ApplyDownMigrations(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            foreach (var migrationInfo in GetDownMigrations(assembly).Reverse())
            {
                _migrationRunner.Down(migrationInfo.Migration);
                var hasMigrationsToApplyDown = _migrationRunner.HasMigrationsToApplyDown(migrationInfo.Version);
                if (hasMigrationsToApplyDown == false)
                {
                    _versionLoader.Value.DeleteVersion(migrationInfo.Version);
                }
            }
        }

        /// <summary>
        /// Executes down expressions for the passed migration
        /// </summary>
        /// <param name="migration">Migration to rollback</param>
        public void DownMigration(IMigration migration)
        {
            if (migration is null)
                throw new ArgumentNullException(nameof(migration));

            var migrationInfo = _migrationRunnerConventions.GetMigrationInfoForMigration(migration);

            _migrationRunner.Down(migrationInfo.Migration);
            _versionLoader.Value.DeleteVersion(migrationInfo.Version);
        }

        /// <summary>
        /// Executes up expressions for the passed migration
        /// </summary>
        /// <param name="migration">Migration to apply</param>
        public void UpMigration(IMigration migration)
        {
            if (migration is null)
                throw new ArgumentNullException(nameof(migration));

            var migrationInfo = _migrationRunnerConventions.GetMigrationInfoForMigration(migration);
            _migrationRunner.Up(migrationInfo.Migration);

            _versionLoader.Value
                    .UpdateVersionInfo(migrationInfo.Version, migrationInfo.Description ?? migrationInfo.Migration.GetType().Name);
        }

        #endregion

    }
}
