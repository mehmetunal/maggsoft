using Maggsoft.Data.Npgsql;
using System.Collections.Generic;

namespace Maggsoft.ExampleTest.Entity
{
    public class User: BaseEntity
    {
        public string Text { get; set; }

        public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}
