using ERP.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the RolePermission entity.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "identity");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Id)
            .HasColumnName("RolePermissionId")
            .ValueGeneratedOnAdd();

        builder.Property(rp => rp.Permission)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rp => rp.GrantedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationship is configured in Role configuration

        // Indexes
        builder.HasIndex(rp => new { rp.RoleId, rp.Permission })
            .IsUnique();

        builder.HasIndex(rp => rp.Permission);
    }
}
