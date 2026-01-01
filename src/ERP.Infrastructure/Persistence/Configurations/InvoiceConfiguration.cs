using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices", "bill");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("InvoiceId")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.TenantId)
            .IsRequired();

        // Value object mapping for InvoiceNumber
        builder.Property(i => i.InvoiceNumber)
            .HasConversion(
                v => v.Value,
                v => new InvoiceNumber(v))
            .HasColumnName("InvoiceNumber")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.ClientId)
            .IsRequired();

        builder.Property(i => i.ProjectId)
            .IsRequired(false);

        builder.Property(i => i.BillingMode)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(i => i.InvoiceDate)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .IsRequired();

        builder.Property(i => i.PoNumber)
            .HasMaxLength(100);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.TaxRate)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasMaxLength(3)
            .IsRequired();

        // Value object mapping for AmountPaid (Money)
        builder.OwnsOne(i => i.AmountPaid, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("AmountPaid")
                .HasPrecision(18, 2)
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("AmountPaidCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(i => i.SentDate);
        builder.Property(i => i.PostedDate);
        builder.Property(i => i.PaidDate);
        builder.Property(i => i.VoidedDate);

        builder.Property(i => i.VoidReason)
            .HasMaxLength(500);

        // Audit fields
        builder.Property(i => i.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(i => i.ModifiedDate);

        builder.Property(i => i.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasMany(i => i.LineItems)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => i.TenantId)
            .HasDatabaseName("IX_Invoices_TenantId");

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber })
            .IsUnique()
            .HasDatabaseName("IX_Invoices_TenantId_InvoiceNumber");

        builder.HasIndex(i => i.ClientId)
            .HasDatabaseName("IX_Invoices_ClientId");

        builder.HasIndex(i => i.ProjectId)
            .HasDatabaseName("IX_Invoices_ProjectId");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("IX_Invoices_Status");

        builder.HasIndex(i => i.InvoiceDate)
            .HasDatabaseName("IX_Invoices_InvoiceDate");

        builder.HasIndex(i => i.DueDate)
            .HasDatabaseName("IX_Invoices_DueDate");
    }
}
