using Maggsoft.ExampleTest.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Maggsoft.ExampleTest.Context
{
    public class NpgsqlContext : DbContext
    {
        public NpgsqlContext(DbContextOptions<NpgsqlContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
               .HasQueryFilter(m => EF.Property<string>(m, nameof(m.Name)) == "mehmet");

            //modelBuilder.Entity<Table>()
            //    .HasQueryFilter(m => EF.Property<bool>(m, nameof(m.IsPublish)) == true);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
    }
}
