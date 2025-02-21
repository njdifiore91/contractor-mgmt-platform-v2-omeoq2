using Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Backend.Infrastructure.Data
{
    /// <summary>
    /// Central database context managing entity operations, relationships, and optimizations.
    /// Implements comprehensive data access patterns with built-in audit trails and security enforcement.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        #region DbSet Properties

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Inspector> Inspectors { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<DrugTest> DrugTests { get; set; }
        public DbSet<QuickLink> QuickLinks { get; set; }
        public DbSet<User> Users { get; set; }

        #endregion

        #region Configuration Properties

        /// <summary>
        /// Command timeout for database operations in seconds
        /// </summary>
        public int CommandTimeout { get; private set; }

        /// <summary>
        /// Controls automatic detection of changes for entity tracking
        /// </summary>
        public bool AutoDetectChanges { get; private set; }

        #endregion

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            CommandTimeout = 30;
            AutoDetectChanges = true;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            ChangeTracker.AutoDetectChangesEnabled = true;
            ChangeTracker.LazyLoadingEnabled = true;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            // Apply global query filters for soft delete
            modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Inspector>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Equipment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<DrugTest>().HasQueryFilter(e => !e.IsDeleted);

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                
                // Configure relationship with contracts
                entity.HasMany(e => e.Contracts)
                      .WithOne(c => c.Customer)
                      .HasForeignKey(c => c.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Inspector entity
            modelBuilder.Entity<Inspector>(entity =>
            {
                entity.ToTable("Inspectors");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.InspectorId).IsUnique();
                
                // Configure relationships
                entity.HasMany<DrugTest>()
                      .WithOne(d => d.Inspector)
                      .HasForeignKey(d => d.InspectorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany<Equipment>()
                      .WithOne(e => e.AssignedInspector)
                      .HasForeignKey(e => e.AssignedToInspectorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Equipment entity
            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.ToTable("Equipment");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SerialNumber).IsUnique();
                
                entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Condition).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(50);
            });

            // Configure DrugTest entity
            modelBuilder.Entity<DrugTest>(entity =>
            {
                entity.ToTable("DrugTests");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.InspectorId, e.TestDate });
                
                entity.Property(e => e.TestType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Frequency).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Result).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Company).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Comment).HasMaxLength(500);
                
                entity.Property(e => e.RowVersion).IsRowVersion();
            });

            // Configure QuickLink entity
            modelBuilder.Entity<QuickLink>(entity =>
            {
                entity.ToTable("QuickLinks");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Order);
                
                entity.Property(e => e.Label).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Link).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ModifiedBy).HasMaxLength(256);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.SecurityStamp).IsRequired();
                entity.Property(e => e.ConcurrencyStamp).IsRequired();
                
                // Configure collections
                entity.Property(e => e.Roles)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                
                entity.Property(e => e.Permissions)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}