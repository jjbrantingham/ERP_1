using ERP.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Role entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "identity");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("RoleId")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.NormalizedName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsSystemRole)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.Property(r => r.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.CreatedBy)
            .IsRequired(false);

        builder.Property(r => r.ModifiedDate)
            .IsRequired(false);

        builder.Property(r => r.ModifiedBy)
            .IsRequired(false);

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        // Configure relationships
        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => new { r.TenantId, r.NormalizedName })
            .IsUnique();

        builder.HasIndex(r => r.IsActive);
        builder.HasIndex(r => r.IsSystemRole);

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);
    }
}
