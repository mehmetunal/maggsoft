using FluentMigrator;
using Maggsoft.Data.Migration.Attribute;
using Maggsoft.Data.Mssql;
using System;

namespace Maggsoft.ExampleTest.Migrations;

[MaggsoftMigration("2023/07/13 21:37:00", "Database Init ")]
public class AddTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
         .WithColumn(nameof(BaseEntity.Id)).AsGuid().PrimaryKey().WithDefaultValue(SystemMethods.NewSequentialId).NotNullable()
        .WithColumn(nameof(BaseEntity.IsPublish)).AsBoolean().WithDefaultValue(true).NotNullable()
        .WithColumn(nameof(BaseEntity.IsDeleted)).AsBoolean().WithDefaultValue(false).NotNullable()
        .WithColumn(nameof(BaseEntity.CreatedDate)).AsDateTime().WithDefaultValue(DateTime.Now).NotNullable()
        .WithColumn(nameof(BaseEntity.CreatorIP)).AsString()
        .WithColumn(nameof(BaseEntity.CreatorUserId)).AsGuid().NotNullable()
        .WithColumn(nameof(BaseEntity.ModifiedDate)).AsDateTime().Nullable()
        .WithColumn(nameof(BaseEntity.ModifierIP)).AsString().Nullable()
        .WithColumn(nameof(BaseEntity.ModifierUserId)).AsGuid().Nullable()
        .WithColumn(nameof(BaseEntity.DisplayOrder)).AsInt16().NotNullable().WithDefaultValue(0)
        .WithColumn("Text").AsString();

        Create.Table("UserLogs")
            .WithColumn(nameof(BaseEntity.Id)).AsGuid().PrimaryKey().WithDefaultValue(SystemMethods.NewSequentialId).NotNullable()
            .WithColumn(nameof(BaseEntity.IsPublish)).AsBoolean().WithDefaultValue(true).NotNullable()
            .WithColumn(nameof(BaseEntity.IsDeleted)).AsBoolean().WithDefaultValue(false).NotNullable()
            .WithColumn(nameof(BaseEntity.CreatedDate)).AsDateTime().WithDefaultValue(DateTime.Now).NotNullable()
            .WithColumn(nameof(BaseEntity.CreatorIP)).AsString()
            .WithColumn(nameof(BaseEntity.CreatorUserId)).AsGuid().NotNullable()
            .WithColumn(nameof(BaseEntity.ModifiedDate)).AsDateTime().Nullable()
            .WithColumn(nameof(BaseEntity.ModifierIP)).AsString().Nullable()
            .WithColumn(nameof(BaseEntity.ModifierUserId)).AsGuid().Nullable()
            .WithColumn(nameof(BaseEntity.DisplayOrder)).AsInt16().NotNullable().WithDefaultValue(0).WithColumn("Text").AsString()
            .WithColumn("UserId").AsGuid().ForeignKey("Users", "Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Index("IX_UserLogs_UserId")
            .OnTable("UserLogs")
            .OnColumn("UserId")
            .Ascending()
            .WithOptions()
            .NonClustered();

        // Execute.EmbeddedScript("\"uuid-ossp\"");
        /*
        
        https://fluentmigrator.github.io/articles/migration/migration-attribute-custom.html
        https://fluentmigrator.github.io/articles/version-table-metadata.html

        Execute.Script("myscript.sql");
        Execute.EmbeddedScript("UpdateLegacySP.sql");
        Execute.Sql("DELETE TABLE Users");

        https://fluentmigrator.github.io/articles/migration-example.html
        https://fluentmigrator.github.io/articles/fluent-interface.html

        Insert.IntoTable("User").Row(new { Text = "Memoli" });
        Insert.IntoTable("TestTable").Row(new { Name = new NonUnicodeString("ansi string") });

        Update.Table("Users").Set(new { Name = "John" }).Where(new { Name = "Johnanna" });
        
        Update.Table("Bar")
            .Set(new { SomeDate = DateTime.Today })
            .AllRows();

        Delete.Table("Users");
        Delete.Column("AllowSubscription").Column("SubscriptionDate").FromTable("Users");
        
        Rename.Table("Users").To("UsersNew");
        Rename.Column("LastName").OnTable("Users").To("Surname");
        
        Delete.FromTable("Users").AllRows(); // delete all rows
        Delete.FromTable("Users").Row(new { FirstName = "John" }); // delete all rows with FirstName==John
        Delete.FromTable("Users").IsNull("Username"); //Delete all rows where Username is null

        Alter.Table("Bar")
           .AddColumn("SomeDate")
           .AsDateTime()
           .Nullable();

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
