using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ExpenseReport entity.
/// </summary>
public class ExpenseReportConfiguration : IEntityTypeConfiguration<ExpenseReport>
{
    public void Configure(EntityTypeBuilder<ExpenseReport> builder)
    {
        builder.ToTable("ExpenseReports", "te");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ExpenseReportId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.EmployeeId)
            .IsRequired();

        builder.Property(e => e.ReportNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Purpose)
            .HasMaxLength(500);

        builder.Property(e => e.ReportDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<byte>();

        // Value object: Money (TotalAmount)
        builder.OwnsOne(e => e.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.Property(e => e.SubmittedDate);

        builder.Property(e => e.ApprovedDate);

        builder.Property(e => e.ApprovedByUserId);

        builder.Property(e => e.ApprovalComments)
            .HasMaxLength(4000);

        builder.Property(e => e.ReimbursedDate);

        builder.Property(e => e.Notes)
            .HasMaxLength(4000);

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

        // Relationships
        builder.HasMany(e => e.Items)
            .WithOne(i => i.ExpenseReport)
            .HasForeignKey("ExpenseReportId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ERP.Domain.HR.Entities.Employee>()
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => new { e.TenantId, e.ReportNumber })
            .IsUnique()
            .HasDatabaseName("IX_ExpenseReports_TenantId_ReportNumber");

        builder.HasIndex(e => e.EmployeeId)
            .HasDatabaseName("IX_ExpenseReports_EmployeeId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_ExpenseReports_Status");

        builder.HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("IX_ExpenseReports_TenantId_Status");

        builder.HasIndex(e => e.ReportDate)
            .HasDatabaseName("IX_ExpenseReports_ReportDate");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
