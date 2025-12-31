using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Project entity.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", "pm");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("ProjectId")
            .ValueGeneratedOnAdd();

        // Value object: ProjectNumber
        builder.OwnsOne(p => p.ProjectNumber, pn =>
        {
            pn.Property(n => n.Value)
                .HasColumnName("ProjectNumber")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.Property(p => p.ClientId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.ProjectType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(p => p.BillingMode)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate);

        // Value object: Money (Budget)
        builder.OwnsOne(p => p.Budget, budget =>
        {
            budget.Property(m => m.Amount)
                .HasColumnName("BudgetAmount")
                .HasPrecision(18, 2);
            budget.Property(m => m.Currency)
                .HasColumnName("BudgetCurrency")
                .HasMaxLength(3);
        });

        builder.Property(p => p.ProjectManagerId);

        builder.Property(p => p.Notes)
            .HasMaxLength(4000);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.ProjectNumber })
            .IsUnique()
            .HasDatabaseName("IX_Projects_TenantId_ProjectNumber");

        builder.HasIndex(p => p.ClientId)
            .HasDatabaseName("IX_Projects_ClientId");

        builder.HasIndex(p => p.ProjectManagerId)
            .HasDatabaseName("IX_Projects_ProjectManagerId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Projects_Status");

        builder.HasIndex(p => p.ProjectType)
            .HasDatabaseName("IX_Projects_ProjectType");

        builder.HasIndex(p => new { p.TenantId, p.Status })
            .HasDatabaseName("IX_Projects_TenantId_Status");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
