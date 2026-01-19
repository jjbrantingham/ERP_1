using ERP.Domain.PM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for WBSItem entity.
/// </summary>
public class WBSItemConfiguration : IEntityTypeConfiguration<WBSItem>
{
    public void Configure(EntityTypeBuilder<WBSItem> builder)
    {
        builder.ToTable("WBSItems", "pm");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("WBSItemId")
            .ValueGeneratedOnAdd();

        builder.Property(w => w.ProjectId)
            .IsRequired();

        builder.Property(w => w.ParentId);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(4000);

        builder.Property(w => w.DisplayOrder)
            .IsRequired();

        builder.Property(w => w.EstimatedHours)
            .HasPrecision(18, 2);

        builder.Property(w => w.ActualHours)
            .HasPrecision(18, 2);

        builder.Property(w => w.PercentComplete)
            .HasPrecision(5, 2);

        // Value object: Money (Budget)
        builder.OwnsOne(w => w.Budget, budget =>
        {
            budget.Property(m => m.Amount)
                .HasColumnName("BudgetAmount")
                .HasPrecision(18, 2);
            budget.Property(m => m.Currency)
                .HasColumnName("BudgetCurrency")
                .HasMaxLength(3);
        });

        builder.Property(w => w.StartDate);

        builder.Property(w => w.EndDate);

        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(w => w.TenantId)
            .IsRequired();

        builder.Property(w => w.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(w => w.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(w => w.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(w => w.Project)
            .WithMany()
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Parent)
            .WithMany()
            .HasForeignKey(w => w.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(w => new { w.ProjectId, w.Code })
            .IsUnique()
            .HasDatabaseName("IX_WBSItems_ProjectId_Code");

        builder.HasIndex(w => w.ParentId)
            .HasDatabaseName("IX_WBSItems_ParentId");

        builder.HasIndex(w => new { w.ProjectId, w.DisplayOrder })
            .HasDatabaseName("IX_WBSItems_ProjectId_DisplayOrder");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
