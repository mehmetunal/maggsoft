using Maggsoft.Data.Npgsql;
using System;

namespace Maggsoft.ExampleTest.Entity
{
    public class User 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
