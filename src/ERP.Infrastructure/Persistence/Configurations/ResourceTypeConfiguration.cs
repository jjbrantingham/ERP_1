using ERP.Domain.HR.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ResourceType entity.
/// </summary>
public class ResourceTypeConfiguration : IEntityTypeConfiguration<ResourceType>
{
    public void Configure(EntityTypeBuilder<ResourceType> builder)
    {
        builder.ToTable("ResourceTypes", "hr");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .HasColumnName("ResourceTypeId")
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.Description)
            .HasMaxLength(500);

        builder.Property(rt => rt.Code)
            .HasMaxLength(20);

        builder.Property(rt => rt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(rt => rt.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasMany(rt => rt.Rates)
            .WithOne(r => r.ResourceType)
            .HasForeignKey(r => r.ResourceTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(rt => new { rt.TenantId, rt.Name })
            .IsUnique();

        builder.HasIndex(rt => new { rt.TenantId, rt.Code })
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        builder.HasIndex(rt => rt.IsActive);
        builder.HasIndex(rt => rt.DisplayOrder);

        // Ignore domain events
        builder.Ignore(rt => rt.DomainEvents);

        // Row version for optimistic concurrency
        builder.Property(rt => rt.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(rt => rt.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
