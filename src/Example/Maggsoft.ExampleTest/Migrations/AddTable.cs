using FluentMigrator;
using Maggsoft.Data.Migration.Attribute;

namespace Maggsoft.ExampleTest.Migrations
{
    [MaggsoftMigration("2023/07/13 21:37:00",maggsoftVersion:"v1")]
    public class AddTable : Migration
    {
        public override void Up()
        {
            Create.Table("User")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Text").AsString();

            Create.Table("Log")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Text").AsString()
                .WithColumn("UserId").AsInt64().ForeignKey("User", "Id");

            Create.Index("IX_Log_UserId")
                .OnTable("Log")
                .OnColumn("UserId")
                .Ascending()
                .WithOptions()
                .NonClustered();

            Insert.IntoTable("User").Row(new { Text = "Memoli" });
            //Bunu yapıcam https://fluentmigrator.github.io/articles/migration/migration-attribute-custom.html


            //https://fluentmigrator.github.io/articles/version-table-metadata.html
            //Execute.Script("myscript.sql");
            //Execute.EmbeddedScript("UpdateLegacySP.sql");
            //Execute.Sql("DELETE TABLE Users");
            //https://fluentmigrator.github.io/articles/migration-example.html
            //https://fluentmigrator.github.io/articles/fluent-interface.html

            //Update.Table("Users").Set(new { Name = "John" }).Where(new { Name = "Johnanna" });

            /*
            
            Delete.Table("Users");

            Delete.Column("AllowSubscription").Column("SubscriptionDate").FromTable("Users");

            Rename.Table("Users").To("UsersNew");
            Rename.Column("LastName").OnTable("Users").To("Surname");

            Delete.FromTable("Users").AllRows(); // delete all rows
            Delete.FromTable("Users").Row(new { FirstName = "John" }); // delete all rows with FirstName==John
            Delete.FromTable("Users").IsNull("Username"); //Delete all rows where Username is null

             Update.Table("Users").Set(new { Name = "John" }).Where(new { Name = "Johnanna" });

             Insert.IntoTable("TestTable").Row(new { Name = new NonUnicodeString("ansi string") });

             Alter.Table("Bar")
                .AddColumn("SomeDate")
                .AsDateTime()
                .Nullable();

            Update.Table("Bar")
                .Set(new { SomeDate = DateTime.Today })
                .AllRows();

             IfDatabase("SqlServer", "Postgres")
                .Create.Table("Users")
                .WithIdColumn()
                .WithColumn("Name").AsString().NotNullable();

            IfDatabase("Sqlite")
                .Create.Table("Users")
                .WithColumn("Id").AsInt16().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable();

             if (!Schema.Table("Users").Column("FirstName").Exists())
             {
                 this.Create.Column("FirstName").OnTable("Users").AsAnsiString(128).Nullable();
             }
             
             */
        }

        public override void Down()
        {
            Delete.Table("User");
        }
    }
}
