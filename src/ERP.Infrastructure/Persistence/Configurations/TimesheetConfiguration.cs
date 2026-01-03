using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Timesheet entity.
/// </summary>
public class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
{
    public void Configure(EntityTypeBuilder<Timesheet> builder)
    {
        builder.ToTable("Timesheets", "te");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("TimesheetId")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.EmployeeId)
            .IsRequired();

        builder.Property(t => t.PeriodStart)
            .IsRequired();

        builder.Property(t => t.PeriodEnd)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(t => t.TotalHours)
            .HasPrecision(18, 2);

        builder.Property(t => t.SubmittedDate);

        builder.Property(t => t.ApprovedDate);

        builder.Property(t => t.ApprovedByUserId);

        builder.Property(t => t.ApprovalComments)
            .HasMaxLength(4000);

        builder.Property(t => t.Notes)
            .HasMaxLength(4000);

        // Audit fields
        builder.Property(t => t.TenantId)
            .IsRequired();

        builder.Property(t => t.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasMany(t => t.Entries)
            .WithOne(e => e.Timesheet)
            .HasForeignKey("TimesheetId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ERP.Domain.HR.Entities.Employee>()
            .WithMany()
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.EmployeeId)
            .HasDatabaseName("IX_Timesheets_EmployeeId");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_Timesheets_Status");

        builder.HasIndex(t => new { t.EmployeeId, t.PeriodStart, t.PeriodEnd })
            .HasDatabaseName("IX_Timesheets_EmployeeId_Period");

        builder.HasIndex(t => new { t.TenantId, t.Status })
            .HasDatabaseName("IX_Timesheets_TenantId_Status");

        // Composite indexes for date range queries with status
        builder.HasIndex(t => new { t.Status, t.WeekStartDate })
            .HasDatabaseName("IX_Timesheets_Status_WeekStartDate");

        builder.HasIndex(t => new { t.Status, t.WeekEndDate })
            .HasDatabaseName("IX_Timesheets_Status_WeekEndDate");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
