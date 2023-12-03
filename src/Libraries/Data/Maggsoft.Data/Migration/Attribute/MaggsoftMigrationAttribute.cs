using Maggsoft.Core.Exceptions;
using System.Globalization;

namespace Maggsoft.Data.Migration.Attribute;

public class MaggsoftMigrationAttribute : FluentMigrator.MigrationAttribute
{
    #region Fields

    protected readonly MigrationConfig _config;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the NopMigrationAttribute class
    /// </summary>
    /// <param name="dateTime">The migration date time string to convert on version</param>
    public MaggsoftMigrationAttribute(string dateTime) :
        this(new MigrationConfig
        {
            DateTime = dateTime
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the NopMigrationAttribute class
    /// </summary>
    /// <param name="dateTime">The migration date time string to convert on version</param>
    /// <param name="description">The migration description</param>
    /// <param name="nopVersion">nopCommerce full version</param>

    public MaggsoftMigrationAttribute(string dateTime, string description = null, string maggsoftVersion = null) :
        this(new MigrationConfig
        {
            DateTime = dateTime,
            Description = description,
            MaggsoftVersion= maggsoftVersion
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the NopMigrationAttribute class
    /// </summary>
    /// <param name="config">The migration configuration data</param>
    protected MaggsoftMigrationAttribute(MigrationConfig config) : base(config.Version, config.Description)
    {
        _config = config;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the value which indicate is this schema migration
    /// </summary>
    ///<remarks>
    /// If set to true than this migration will apply right after the migration runner will become available.
    /// Do not us dependency injection in migrations that are marked as schema migration,
    /// because IoC container not ready yet.
    ///</remarks>
    public virtual bool IsSchemaMigration
    {
        get => _config.IsSchemaMigration;
        protected set => _config.IsSchemaMigration = value;
    }


    /// <summary>
    /// Gets the flag which indicate whether the migration should be applied into DB on the debug mode
    /// </summary>
    public virtual bool ApplyInDbOnDebugMode
    {
        get => _config.ApplyInDbOnDebugMode;
        protected set => _config.ApplyInDbOnDebugMode = value;
    }

    #endregion


    #region Nested class
    protected partial class MigrationConfig
    {
        #region Fields

        protected long? _version;
        protected string _description;

        #endregion

        /// <summary>
        /// Gets or sets the migration date time string to convert on version
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// nopCommerce full version
        /// </summary>
        public string MaggsoftVersion { get; set; }

        /// <summary>
        /// Gets or sets the migration version
        /// </summary>
        public virtual long Version
        {
            get
            {
                if (_version.HasValue)
                    return _version.Value;

                if (string.IsNullOrEmpty(DateTime))
                    throw new MaggsoftException("One of the following properties must be initialized: either Version or DateTime");

                var version = System.DateTime
                    .ParseExact(DateTime, MaggsoftMigrationDefaults.DateFormats, CultureInfo.InvariantCulture).Ticks;

                return version;
            }
            set => _version = value;
        }

        /// <summary>
        /// Gets or sets the migration description
        /// </summary>
        public virtual string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(_description))
                    return _description;


                if (string.IsNullOrEmpty(MaggsoftVersion))
                    throw new MaggsoftException("One of the following properties must be initialized: either Description or MaggsoftVersion");

                string description = string.Format(MaggsoftMigrationDefaults.UpdateMigrationDescription, MaggsoftVersion);

                return description;
            }
            set => _description = value;
        }

        /// <summary>
        /// Gets the flag which indicate whether the migration should be applied into DB on the debug mode
        /// </summary>
        public bool ApplyInDbOnDebugMode { get; set; } = true;

        /// <summary>
        /// Gets or sets the value which indicate is this schema migration
        /// </summary>
        ///<remarks>
        /// If set to true than this migration will apply right after the migration runner will become available.
        /// Do not us dependency injection in migrations that are marked as schema migration,
        /// because IoC container not ready yet.
        ///</remarks>
        public virtual bool IsSchemaMigration { get; set; }
    }
    #endregion

}
