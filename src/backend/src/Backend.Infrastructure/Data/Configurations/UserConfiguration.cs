using Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration class for the User entity that defines
    /// database mappings, constraints, and optimized indexing strategies.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table configuration
            builder.ToTable("Users");

            // Primary key
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .UseIdentityColumn();

            // Basic properties with constraints
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.EmailConfirmed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.SecurityStamp)
                .IsRequired();

            builder.Property(u => u.ConcurrencyStamp)
                .IsRequired()
                .IsConcurrencyToken();

            // JSON columns for roles and permissions
            builder.Property(u => u.Roles)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(u => u.Permissions)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            // Status and tracking fields
            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(u => u.LastLoginAt)
                .IsRequired(false);

            builder.Property(u => u.ReceiveEmails)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.LastPasswordChangeAt)
                .IsRequired(false);

            builder.Property(u => u.RequirePasswordChange)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes for optimized querying
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => new { u.FirstName, u.LastName })
                .HasDatabaseName("IX_Users_FullName");

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            builder.HasIndex(u => u.LastLoginAt)
                .HasDatabaseName("IX_Users_LastLoginAt");

            builder.HasIndex(u => u.CreatedAt)
                .HasDatabaseName("IX_Users_CreatedAt");

            // Full-text search index for name fields
            builder.HasIndex(u => new { u.FirstName, u.LastName, u.Email })
                .HasDatabaseName("IX_Users_Search");
        }
    }
}