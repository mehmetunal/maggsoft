using Maggsoft.Data.Npgsql;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Maggsoft.Mssql.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder BaseModelBuilder<Table>(this ModelBuilder builder) where Table : BaseEntity
        {
            builder.Entity<Table>().HasKey(p => p.Id);

            /*
             HasDefaultValue(true);
             modelBuilder.Entity<Company>()
                         .HasMany(c => c.Employees)
                         .WithOne(e => e.Company)
                         .HasConstraintName("MyFKConstraint");
             
             */
            //IsUnique  => Benzersiz değerse indexde bunu ekleyebiliriz.
            //builder.Entity<Table>()
            //   .HasIndex(p => new { p.CreatedDate, p.ModifiedDate }, name: $"IX_{nameof(Table)}_CreatedDate");

            builder.Entity<Table>()
                .HasIndex(p => p.CreatedDate, name: $"IX_{nameof(Table)}_CreatedDate");
            builder.Entity<Table>()
                .HasIndex(p => p.IsDeleted, name: $"IX_{nameof(Table)}_IsDeleted");
            builder.Entity<Table>()
                .HasIndex(p => p.IsPublish, name: $"IX_{nameof(Table)}_IsPublish");


            builder.Entity<Table>().Property(p => p.Id).HasDefaultValue()
                .ValueGeneratedOnAdd();

            builder.Entity<Table>().Property(p => p.CreatedDate)
                .IsRequired();

            builder.Entity<Table>().Property(p => p.CreatorIP)
                .HasMaxLength(50)
                .IsRequired();

            builder.Entity<Table>().Property(p => p.CreatorUserId)
                .IsRequired();

            builder.Entity<Table>().Property(p => p.ModifiedDate);

            builder.Entity<Table>().Property(p => p.ModifierIP)
                .HasMaxLength(50);

            builder.Entity<Table>().Property(p => p.ModifierUserId);

            builder.Entity<Table>()
                .HasQueryFilter(m => EF.Property<bool>(m, nameof(m.IsDeleted)) == false);

            builder.Entity<Table>()
                .HasQueryFilter(m => EF.Property<bool>(m, nameof(m.IsPublish)) == true);

            return builder;

        }


        public static async Task EnableIdentityInsertAsync<T>(this DbContext context) => await SetIdentityInsertAsync<T>(context, true);
        public static async Task DisableIdentityInsertAsync<T>(this DbContext context) => await SetIdentityInsertAsync<T>(context, false);
        private static async Task SetIdentityInsertAsync<T>([NotNull] DbContext context, bool enable)
        {
            //alter table public.category drop constraint "PK_category"
            //ALTER TABLE public.category ADD CONSTRAINT "PK_category"  PRIMARY KEY("Id");

            if (context == null) throw new ArgumentNullException(nameof(context));

            var entityType = context.Model.FindEntityType(typeof(T));

            var primaryKey = entityType.FindPrimaryKey();
            var primaryKeyDefaultName = primaryKey.GetDefaultName();

            var schema = entityType.GetSchema();
            var tableName = entityType.GetTableName();
            var storeObjectIdentifier = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(tableName, schema);
            var primaryKeyColumnName = primaryKey.Properties
                .Select(x => x.GetColumnName(storeObjectIdentifier))
                .FirstOrDefault();


            var tb = $"{tableName}";
            if (!string.IsNullOrEmpty(schema))
            {
                tb = $"{schema}.{tableName}";
            }

            if (enable == true)
            {
                await context.Database.ExecuteSqlRawAsync($"alter table {tb} drop constraint \"{primaryKeyDefaultName}\"");
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync($"alter table {tb} ADD constraint \"{primaryKeyDefaultName}\" PRIMARY KEY(\"{primaryKeyColumnName}\")");
            }
        }

        public static void EnableIdentityInsert<T>(this DbContext context) => SetIdentityInsert<T>(context, true);
        public static void DisableIdentityInsert<T>(this DbContext context) => SetIdentityInsert<T>(context, false);
        private static void SetIdentityInsert<T>([NotNull] DbContext context, bool enable)
        {
            //alter table public.category drop constraint "PK_category"
            //ALTER TABLE public.category ADD CONSTRAINT "PK_category"  PRIMARY KEY("Id");

            if (context == null) throw new ArgumentNullException(nameof(context));

            var entityType = context.Model.FindEntityType(typeof(T));

            var primaryKey = entityType.FindPrimaryKey();
            var primaryKeyDefaultName = primaryKey.GetDefaultName();

            var schema = entityType.GetSchema();
            var tableName = entityType.GetTableName();
            var storeObjectIdentifier = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(tableName, schema);
            var primaryKeyColumnName = primaryKey.Properties
                .Select(x => x.GetColumnName(storeObjectIdentifier))
                .FirstOrDefault();


            var tb = $"{tableName}";
            if (!string.IsNullOrEmpty(schema))
            {
                tb = $"{schema}.{tableName}";
            }

            if (enable == true)
            {
                context.Database.ExecuteSqlRaw($"alter table {tb} drop constraint \"{primaryKeyDefaultName}\"");
            }
            else
            {
                context.Database.ExecuteSqlRaw($"alter table {tb} ADD constraint \"{primaryKeyDefaultName}\" PRIMARY KEY(\"{primaryKeyColumnName}\")");
                //context.Database.ExecuteSqlRaw($"alter table {tb} ALTER COLUMN \"{primaryKeyColumnName}\" SET DATA TYPE UUID USING(uuid_generate_v4())");
            }
        }
    }
}
