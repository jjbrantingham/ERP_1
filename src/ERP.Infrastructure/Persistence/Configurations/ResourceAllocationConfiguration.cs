using ERP.Domain.PM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ResourceAllocation entity
/// </summary>
public class ResourceAllocationConfiguration : IEntityTypeConfiguration<ResourceAllocation>
{
    public void Configure(EntityTypeBuilder<ResourceAllocation> builder)
    {
        builder.ToTable("ResourceAllocations", "pm");

        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.Id)
            .HasColumnName("ResourceAllocationId")
            .ValueGeneratedOnAdd();

        builder.Property(ra => ra.ProjectId)
            .IsRequired();

        builder.Property(ra => ra.EmployeeId)
            .IsRequired();

        builder.Property(ra => ra.StartDate)
            .IsRequired();

        builder.Property(ra => ra.EndDate);

        builder.Property(ra => ra.AllocatedHoursPerWeek)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(ra => ra.Role)
            .HasMaxLength(100);

        builder.Property(ra => ra.Notes)
            .HasMaxLength(4000);

        builder.Property(ra => ra.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(ra => ra.TenantId)
            .IsRequired();

        builder.Property(ra => ra.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(ra => ra.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(ra => ra.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(ra => new { ra.TenantId, ra.ProjectId })
            .HasDatabaseName("IX_ResourceAllocations_TenantId_ProjectId");

        builder.HasIndex(ra => new { ra.TenantId, ra.EmployeeId })
            .HasDatabaseName("IX_ResourceAllocations_TenantId_EmployeeId");

        builder.HasIndex(ra => ra.IsActive)
            .HasDatabaseName("IX_ResourceAllocations_IsActive");

        builder.HasIndex(ra => new { ra.ProjectId, ra.IsActive })
            .HasDatabaseName("IX_ResourceAllocations_ProjectId_IsActive");
    }
}
