using Maggsoft.ExampleTest.Entity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Maggsoft.ExampleTest.Context
{
    public class AppContext : DbContext
    {
        public AppContext(DbContextOptions<AppContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.LogTo(Console.WriteLine);


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>()
            //   .HasQueryFilter(m => EF.Property<string>(m, nameof(m.Text)) == "mehmet");

            //modelBuilder.Entity<Table>()
            //    .HasQueryFilter(m => EF.Property<bool>(m, nameof(m.IsPublish)) == true);

            ///ignore query context.Users.IgnoreQueryFilters().ToListAsync();


            base.OnModelCreating(modelBuilder);
            //CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
            //modelBuilder.HasPostgresExtension("uuid-ossp");
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
