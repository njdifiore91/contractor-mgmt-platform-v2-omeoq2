using Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// v6.0.0 Microsoft.EntityFrameworkCore
// v6.0.0 Microsoft.EntityFrameworkCore.Metadata.Builders

namespace Backend.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration class for the Equipment entity that defines
    /// database schema, relationships, constraints, and optimizations
    /// </summary>
    public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            // Table configuration
            builder.ToTable("Equipment");

            // Primary key
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Required properties with constraints
            builder.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(e => e.SerialNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.Property(e => e.Condition)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .HasCheckConstraint("CK_Equipment_Condition", 
                    "Condition IN ('New', 'Excellent', 'Good', 'Fair', 'Poor', 'Damaged')");

            // Assignment tracking properties
            builder.Property(e => e.IsOut)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.AssignedToInspectorId)
                .IsRequired(false);

            builder.Property(e => e.AssignedDate)
                .HasColumnType("datetime2(7)")
                .IsRequired(false);

            builder.Property(e => e.AssignedCondition)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            // Return tracking properties
            builder.Property(e => e.ReturnedDate)
                .HasColumnType("datetime2(7)")
                .IsRequired(false);

            builder.Property(e => e.ReturnedCondition)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            // Audit properties
            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2(7)")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2(7)")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            // Relationships
            builder.HasOne<Inspector>()
                .WithMany()
                .HasForeignKey(e => e.AssignedToInspectorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes for performance optimization
            builder.HasIndex(e => e.Model)
                .HasDatabaseName("IX_Equipment_Model");

            builder.HasIndex(e => new { e.SerialNumber, e.CompanyId })
                .IsUnique()
                .HasDatabaseName("IX_Equipment_SerialNumber_CompanyId");

            builder.HasIndex(e => new { e.Model, e.SerialNumber, e.Description })
                .HasDatabaseName("IX_Equipment_List_Covering");

            builder.HasIndex(e => new { e.IsOut, e.AssignedToInspectorId })
                .HasFilter("[IsOut] = 1")
                .HasDatabaseName("IX_Equipment_Assignment");

            // Date constraints
            builder.HasCheckConstraint("CK_Equipment_AssignedDate_Future",
                "AssignedDate IS NULL OR AssignedDate <= GETUTCDATE()");

            builder.HasCheckConstraint("CK_Equipment_ReturnedDate_Future",
                "ReturnedDate IS NULL OR ReturnedDate <= GETUTCDATE()");

            builder.HasCheckConstraint("CK_Equipment_ReturnedDate_AfterAssigned",
                "ReturnedDate IS NULL OR AssignedDate IS NULL OR ReturnedDate >= AssignedDate");
        }
    }
}