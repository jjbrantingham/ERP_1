using ERP.Domain.TE.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for TimesheetEntry entity.
/// </summary>
public class TimesheetEntryConfiguration : IEntityTypeConfiguration<TimesheetEntry>
{
    public void Configure(EntityTypeBuilder<TimesheetEntry> builder)
    {
        builder.ToTable("TimesheetEntries", "te");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("TimesheetEntryId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TimesheetId)
            .IsRequired();

        builder.Property(e => e.ProjectId);

        builder.Property(e => e.WBSItemId);

        builder.Property(e => e.WorkDate)
            .IsRequired();

        builder.Property(e => e.Hours)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.IsBillable)
            .IsRequired()
            .HasDefaultValue(false);

        // Audit fields
        builder.Property(e => e.TenantId)
            .IsRequired();

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Relationships are configured in TimesheetConfiguration

        // Indexes
        builder.HasIndex(e => e.TimesheetId)
            .HasDatabaseName("IX_TimesheetEntries_TimesheetId");

        builder.HasIndex(e => e.ProjectId)
            .HasDatabaseName("IX_TimesheetEntries_ProjectId");

        builder.HasIndex(e => e.WorkDate)
            .HasDatabaseName("IX_TimesheetEntries_WorkDate");

        builder.HasIndex(e => new { e.ProjectId, e.IsBillable })
            .HasDatabaseName("IX_TimesheetEntries_ProjectId_IsBillable");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
