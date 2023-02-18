namespace Maggsoft.Core.DbType.Npgsql
{
    public struct ColumnType
    {
        public const string Serial = "serial";
        public const string Uuid = "uuid";
        public const string Timestamp = "timestamp";
        public const string Numeric = "numeric";
        public const string Varchar = "varchar";
        public const string Integer = "integer";
        public const string Text = "text";
        public const string Boolean = "boolean";
        public const string DateTimeOffset = "time with time zone";
        public const string TimeSpan = "time without time zone";
        public const string DateTime = "date";
        public const string Json = "json";
        public const string Jsonb = "jsonb";
        public const string Xml = "xml";
        public const string String = "citext";
    }
}
