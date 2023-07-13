namespace Maggsoft.Data.Migration.Attribute
{
    /// <summary>
    /// Represents default values related to migration process
    /// </summary>
    public static partial class MaggsoftMigrationDefaults
    {
        /// <summary>
        /// Gets the supported datetime formats
        /// </summary>
        public static string[] DateFormats { get; } = { "yyyy-MM-dd HH:mm:ss", "yyyy.MM.dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss:fffffff", "yyyy.MM.dd HH:mm:ss:fffffff", "yyyy/MM/dd HH:mm:ss:fffffff" };

        /// <summary>
        /// Gets the format string to create the description of update migration
        /// <remarks>
        /// 0 -  Maggsoft full version
        /// 1 - update migration type
        /// </remarks>
        /// </summary>
        public static string UpdateMigrationDescription => " Maggsoft version {0}.";
    }
}
