using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", "bill");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("PaymentId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.PaymentNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.ClientId)
            .IsRequired();

        builder.Property(p => p.InvoiceId)
            .IsRequired(false);

        // Value object mapping for Amount (Money)
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.ClearedDate);

        builder.Property(p => p.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        // Audit fields
        builder.Property(p => p.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.ModifiedDate);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("IX_Payments_TenantId");

        builder.HasIndex(p => new { p.TenantId, p.PaymentNumber })
            .IsUnique()
            .HasDatabaseName("IX_Payments_TenantId_PaymentNumber");

        builder.HasIndex(p => p.ClientId)
            .HasDatabaseName("IX_Payments_ClientId");

        builder.HasIndex(p => p.InvoiceId)
            .HasDatabaseName("IX_Payments_InvoiceId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payments_Status");

        builder.HasIndex(p => p.PaymentDate)
            .HasDatabaseName("IX_Payments_PaymentDate");
    }
}
