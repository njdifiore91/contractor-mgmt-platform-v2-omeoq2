// Microsoft.EntityFrameworkCore v6.0.0
using Microsoft.EntityFrameworkCore;
// Microsoft.EntityFrameworkCore.Metadata.Builders v6.0.0
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Core.Entities;
using System;

namespace Backend.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration class for the Customer entity.
    /// Defines database schema, relationships, and constraints for customer data.
    /// </summary>
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        /// <summary>
        /// Configures the database schema and relationships for the Customer entity.
        /// </summary>
        /// <param name="builder">The entity type builder for Customer configuration</param>
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            // Configure primary key
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            // Configure required string properties with constraints
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            // Create unique index on Code
            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasDatabaseName("IX_Customers_Code");

            // Configure audit properties
            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(c => c.CreatedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            // Configure ContactIds as JSON column
            builder.Property(c => c.ContactIds)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                    v => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.ICollection<int>>(v, new System.Text.Json.JsonSerializerOptions())
                )
                .HasColumnType("nvarchar(max)")
                .HasColumnName("ContactIds");

            // Configure one-to-many relationship with Contracts
            builder.HasMany(c => c.Contracts)
                .WithOne(contract => contract.Customer)
                .HasForeignKey(contract => contract.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create index for performance optimization
            builder.HasIndex(c => new { c.IsActive, c.CreatedAt })
                .HasDatabaseName("IX_Customers_IsActive_CreatedAt");

            // Configure table name
            builder.ToTable("Customers");
        }
    }
}