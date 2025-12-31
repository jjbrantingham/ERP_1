using ERP.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "identity");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("UserId")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        // Configure Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);
        });

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.EmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.TwoFactorEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.LockoutEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.AccessFailedCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LockoutEnd)
            .IsRequired(false);

        builder.Property(u => u.LastLoginDate)
            .IsRequired(false);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        builder.Property(u => u.RefreshTokenExpiry)
            .IsRequired(false);

        builder.Property(u => u.TenantId)
            .IsRequired();

        builder.Property(u => u.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.CreatedBy)
            .IsRequired(false);

        builder.Property(u => u.ModifiedDate)
            .IsRequired(false);

        builder.Property(u => u.ModifiedBy)
            .IsRequired(false);

        builder.Property(u => u.RowVersion)
            .IsRowVersion();

        // Configure relationships
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(u => new { u.TenantId, u.UserName })
            .IsUnique();

        builder.HasIndex(u => new { u.TenantId, u.Email.Value })
            .HasDatabaseName("IX_Users_TenantId_Email")
            .IsUnique();

        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.LastLoginDate);

        // Ignore domain events (handled separately)
        builder.Ignore(u => u.DomainEvents);
    }
}
