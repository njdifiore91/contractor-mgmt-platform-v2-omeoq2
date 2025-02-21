using Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration for the Inspector entity that defines database schema,
    /// relationships, constraints, and optimized indices for inspector data management.
    /// </summary>
    public class InspectorConfiguration : IEntityTypeConfiguration<Inspector>
    {
        public void Configure(EntityTypeBuilder<Inspector> builder)
        {
            // Configure JSON serialization options
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };

            // Primary Key
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedOnAdd();

            // Personal Information
            builder.Property(i => i.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");
            builder.HasIndex(i => i.FirstName);

            builder.Property(i => i.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");
            builder.HasIndex(i => i.LastName);

            builder.Property(i => i.MiddleName)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(i => i.Nickname)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(i => i.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");
            builder.HasIndex(i => i.Email).IsUnique();

            builder.Property(i => i.Phone)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            builder.Property(i => i.DateOfBirth)
                .IsRequired()
                .HasColumnType("date");

            // Professional Information
            builder.Property(i => i.InspectorId)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");
            builder.HasIndex(i => i.InspectorId).IsUnique();

            builder.Property(i => i.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");
            builder.HasIndex(i => i.Status);

            builder.Property(i => i.State)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnType("char(2)");
            builder.HasIndex(i => i.State);

            builder.Property(i => i.Title)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            // JSON Collections with Value Converters
            builder.Property(i => i.Specialties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            builder.Property(i => i.RequiredCertifications)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            // Status Flags
            builder.Property(i => i.HasIssues)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(i => i.NeedsApproval)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(i => i.CertificationRequired)
                .IsRequired()
                .HasDefaultValue(false);

            // Mobilization Details
            builder.Property(i => i.MobilizationDate)
                .HasColumnType("datetime2");

            builder.Property(i => i.HireType)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(i => i.Classification)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(i => i.Department)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(i => i.Function)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(i => i.ProjectLocation)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            // Demobilization Details
            builder.Property(i => i.DemobilizationDate)
                .HasColumnType("datetime2");

            builder.Property(i => i.DemobilizationReason)
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            // Drug Test Information
            builder.Property(i => i.DrugTestIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<int>>(v, jsonOptions) ?? new List<int>())
                .HasColumnType("nvarchar(max)");
            builder.HasIndex(i => i.DrugTestIds);

            builder.Property(i => i.LastDrugTestDate)
                .HasColumnType("datetime2");

            builder.Property(i => i.LastDrugTestResult)
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            // Equipment and Assignment Information
            builder.Property(i => i.AssignedEquipmentIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<int>>(v, jsonOptions) ?? new List<int>())
                .HasColumnType("nvarchar(max)");
            builder.HasIndex(i => i.AssignedEquipmentIds);

            builder.Property(i => i.AssignedCustomers)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            builder.Property(i => i.AssignedContracts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            // Audit Fields
            builder.Property(i => i.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(i => i.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(i => i.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(i => i.UpdatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            // Composite Indices for Performance
            builder.HasIndex(i => new { i.Status, i.State });
            builder.HasIndex(i => new { i.State, i.LastDrugTestDate });
            builder.HasIndex(i => new { i.Status, i.Classification });
        }
    }
}