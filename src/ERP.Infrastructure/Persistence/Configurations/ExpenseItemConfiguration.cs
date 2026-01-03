using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ExpenseItem entity.
/// </summary>
public class ExpenseItemConfiguration : IEntityTypeConfiguration<ExpenseItem>
{
    public void Configure(EntityTypeBuilder<ExpenseItem> builder)
    {
        builder.ToTable("ExpenseItems", "te");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("ExpenseItemId")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.ExpenseReportId)
            .IsRequired();

        builder.Property(i => i.ProjectId);

        builder.Property(i => i.ExpenseDate)
            .IsRequired();

        builder.Property(i => i.Category)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(500);

        // Value object: Money (Amount)
        builder.OwnsOne(i => i.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.Property(i => i.Merchant)
            .HasMaxLength(200);

        builder.Property(i => i.ReceiptNumber)
            .HasMaxLength(100);

        builder.Property(i => i.HasReceipt)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.IsReimbursable)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(i => i.TenantId)
            .IsRequired();

        builder.Property(i => i.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(i => i.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(i => i.RowVersion)
            .IsRowVersion();

        // Relationships are configured in ExpenseReportConfiguration

        // Indexes
        builder.HasIndex(i => i.ExpenseReportId)
            .HasDatabaseName("IX_ExpenseItems_ExpenseReportId");

        builder.Property(i => i.ProjectId);

        builder.HasIndex(i => i.Category)
            .HasDatabaseName("IX_ExpenseItems_Category");

        builder.HasIndex(i => i.ExpenseDate)
            .HasDatabaseName("IX_ExpenseItems_ExpenseDate");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
