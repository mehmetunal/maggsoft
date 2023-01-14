using Dev.Core.DbType.Npgsql;
using Dev.Data.Npgsql.Enum;
using Dev.Data.Npgsql.Identity;
using Dev.Data.Npgsql.Location;
using Dev.Data.Npgsql.Messages;
using Dev.Npgsql.Extensions;
using Microsoft.EntityFrameworkCore;
using System;

namespace Dev.Npgsql.Context
{
    public class NpgsqlContext : DbContext
    {
        #region Ctor
        public NpgsqlContext()
        {

        }
        public NpgsqlContext(DbContextOptions<NpgsqlContext> options)
            : base(options)
        {
            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            //this.ChangeTracker.LazyLoadingEnabled = false;
        }
        #endregion

        #region DbSet

        #region ENUM
        public DbSet<Parameter> Parameter { get; set; }
        public DbSet<ParameterValue> ParameterValue { get; set; }
        #endregion

        #region IDENTITY
        public DbSet<Permission> Permission { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<UserToken> UserToken { get; set; }
        #endregion

        #region LOCATION
        public DbSet<Country> Country { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<County> County { get; set; }
        #endregion

        #endregion

        #region Override 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseLazyLoadingProxies();
            //optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }

        #endregion

        #region Method
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

            #region ENUM
            #region PARAMETER

            modelBuilder.BaseModelBuilder<Parameter>();

            modelBuilder.Entity<Parameter>()
                .Property(p => p.Name)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Parameter>()
                .HasMany(e => e.ParameterValue)
                .WithOne(c => c.Parameter)
                .HasForeignKey(c => c.ParameterId)
                .OnDelete(DeleteBehavior.NoAction);

            #endregion

            #region PARAMETER_VALUE

            modelBuilder.BaseModelBuilder<ParameterValue>();

            modelBuilder.Entity<ParameterValue>()
                .Property(p => p.State)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            modelBuilder.Entity<ParameterValue>()
                .Property(p => p.AccessLevel)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            modelBuilder.Entity<ParameterValue>()
                .Property(p => p.ParameterId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            modelBuilder.Entity<ParameterValue>()
                .Property(p => p.Name)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<ParameterValue>()
                .Property(p => p.Value)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            #endregion

            #endregion

            #region IDENTITY

            #region PERMISSION

            modelBuilder.BaseModelBuilder<Permission>();

            modelBuilder.Entity<Permission>()
                .Property(p => p.State)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            modelBuilder.Entity<Permission>()
                .Property(p => p.AccessLevel)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            modelBuilder.Entity<Permission>()
                .Property(p => p.Name)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Permission>()
                .HasMany(e => e.RolePermission)
                .WithOne(c => c.Permission)
                .HasForeignKey(c => c.PermissionId)
                .OnDelete(DeleteBehavior.NoAction);

            #endregion

            #region ROLE

            modelBuilder.BaseModelBuilder<Role>();

            modelBuilder.Entity<Role>().Property(p => p.State)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            modelBuilder.Entity<Role>().Property(p => p.AccessLevel)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            modelBuilder.Entity<Role>().Property(p => p.Name)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Role>()
                .HasMany(e => e.UserRole)
                .WithOne(c => c.Role)
                .HasForeignKey(c => c.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.RolePermission)
                .WithOne(c => c.Role)
                .HasForeignKey(c => c.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            #endregion

            #region ROLE_PERMISSION

            modelBuilder.BaseModelBuilder<RolePermission>();

            modelBuilder.Entity<RolePermission>()
                .Property(p => p.RoleId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            modelBuilder.Entity<RolePermission>()
                .Property(p => p.PermissionId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            #endregion

            #region USER

            modelBuilder.BaseModelBuilder<User>();

            modelBuilder.Entity<User>()
                .Property(p => p.State)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(p => p.AccessLevel)
                .HasColumnType(ColumnType.Integer)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(p => p.FirstName)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(p => p.LastName)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(p => p.Email)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(p => p.Password)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(p => p.LanguageId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserRole)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserToken)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            #endregion

            #region USER_ROLE

            modelBuilder.BaseModelBuilder<UserRole>();

            modelBuilder.Entity<UserRole>()
                .Property(p => p.UserId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            modelBuilder.Entity<UserRole>()
                .Property(p => p.RoleId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            modelBuilder.Entity<UserRole>()
                .Property(p => p.UserId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            modelBuilder.Entity<UserRole>()
                .Property(p => p.RoleId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            #endregion

            #region USER_TOKEN

            modelBuilder.BaseModelBuilder<UserToken>();

            modelBuilder.Entity<UserToken>()
                .Property(p => p.UserId)
                .HasColumnType(ColumnType.Numeric)
                .HasPrecision(18, 0)
                .IsRequired();

            modelBuilder.Entity<UserToken>()
                .Property(p => p.RefreshToken)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(300)
                .IsRequired();

            modelBuilder.Entity<UserToken>()
                .Property(p => p.RefreshTokenEndDate)
                .HasColumnType(ColumnType.Timestamp)
                .IsRequired();

            #endregion

            #endregion

            #region LOCATION

            #region Countries

            modelBuilder.BaseModelBuilder<Country>();

            modelBuilder.Entity<Country>()
                .Property(p => p.State)
                .HasColumnType(ColumnType.Numeric)
                .IsRequired();

            modelBuilder.Entity<Country>()
                .Property(p => p.Name)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Country>()
                .Property(p => p.Code)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(6)
                .IsRequired();

            modelBuilder.Entity<Country>()
                .HasMany(e => e.Cities)
                .WithOne(c => c.Country)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.NoAction);

            #endregion

            #region Countries

            modelBuilder.BaseModelBuilder<City>();

            modelBuilder.Entity<City>()
                .Property(p => p.State)
                .HasColumnType(ColumnType.Numeric)
                .IsRequired();

            modelBuilder.Entity<City>()
                .Property(p => p.Name)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<City>()
                .Property(p => p.CountryId)
                .HasColumnType(ColumnType.Numeric)
                .IsRequired();

            modelBuilder.Entity<City>()
                .HasMany(e => e.Counties)
                .WithOne(c => c.City)
                .HasForeignKey(c => c.CityId)
                .OnDelete(DeleteBehavior.NoAction);

            #endregion

            #region County

            modelBuilder.BaseModelBuilder<County>();

            modelBuilder.Entity<County>()
                .Property(p => p.State)
                .HasColumnType(ColumnType.Numeric)
                .IsRequired();

            modelBuilder.Entity<County>()
                .Property(p => p.Name)
                .HasColumnType(ColumnType.Varchar)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<County>()
                .Property(p => p.CityId)
                .HasColumnType(ColumnType.Numeric)
                .IsRequired();

            #endregion

            #endregion

            #region EMAILSETTING
            modelBuilder.BaseModelBuilder<EmailAccount>();
            #endregion

            //SEED_DATA
            modelBuilder.Entity<Parameter>().HasData(new Parameter() { }, new Parameter() { });

            //All Decimals will have 18,6 Range
            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //.SelectMany(t => t.GetProperties())
            //.Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            //{
            //    property.SetColumnType("decimal(18,6)");
            //}
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("uuid-ossp");
        }
        #endregion
    }
}
