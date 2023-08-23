using Maggsoft.Core.Entities;
using Maggsoft.Data.Npgsql;
using System;

namespace Maggsoft.ExampleTest.Entity
{
    public class Log : Maggsoft.Data.Npgsql.BaseEntity
    {
        public string Text { get; set; }
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
