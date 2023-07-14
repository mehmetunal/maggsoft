using System;
using System.Collections.Generic;

namespace Maggsoft.ExampleTest.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}
