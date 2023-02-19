using FluentMigrator;
using Maggsoft.ExampleTest.Entity;

namespace Maggsoft.ExampleTest.Migrations
{
    [Migration(20180430121800)]
    public class AddLogTable : Migration
    {
        public override void Up()
        {
            Create.Table("Log")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Text").AsString();
        }

        public override void Down()
        {
            Delete.Table("Log");
        }
    }



    [Migration(202201011201)]
    public class UserLogTable : Migration
    {
        public override void Up()
        {
            Create.Table("user")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Text").AsString();
        }

        public override void Down()
        {
        }
    }
}
