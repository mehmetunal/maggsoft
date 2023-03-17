using Maggsoft.ExampleTest.Entity;
using Microsoft.EntityFrameworkCore;

namespace Maggsoft.ExampleTest.Context
{
    public class NpgsqlContext : DbContext
    {
        public NpgsqlContext(DbContextOptions<NpgsqlContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
