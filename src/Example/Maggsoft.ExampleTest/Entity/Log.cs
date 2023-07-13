using System;

namespace Maggsoft.ExampleTest.Entity
{
    public class Log
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
