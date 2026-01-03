using ERP.Domain.BILL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("InvoiceLineItems", "bill");

        builder.HasKey(il => il.Id);

        builder.Property(il => il.Id)
            .HasColumnName("InvoiceLineItemId")
            .ValueGeneratedOnAdd();

        builder.Property(il => il.InvoiceId)
            .IsRequired();

        builder.Property(il => il.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(il => il.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        // Value object mapping for UnitPrice (Money)
        builder.OwnsOne(il => il.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 2)
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(il => il.DiscountPercent)
            .HasPrecision(5, 2)
            .IsRequired();

        // Audit fields
        builder.Property(il => il.TenantId)
            .IsRequired();

        builder.Property(il => il.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(il => il.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(il => il.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(il => il.InvoiceId)
            .HasDatabaseName("IX_InvoiceLineItems_InvoiceId");
    }
}
