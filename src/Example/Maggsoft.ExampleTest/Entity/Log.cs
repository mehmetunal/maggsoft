using Maggsoft.Data.Npgsql;
using System;

namespace Maggsoft.ExampleTest.Entity
{
    public class Log : BaseEntity
    {
        public string Text { get; set; }
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
