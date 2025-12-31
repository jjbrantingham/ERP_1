using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Vendor entity.
/// </summary>
public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors", "vm");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("VendorId")
            .ValueGeneratedOnAdd();

        // Value object: VendorNumber
        builder.OwnsOne(v => v.VendorNumber, vn =>
        {
            vn.Property(n => n.Value)
                .HasColumnName("VendorNumber")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.VendorType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(v => v.TaxId)
            .HasMaxLength(50);

        builder.Property(v => v.Website)
            .HasMaxLength(200);

        // Value object: Email
        builder.OwnsOne(v => v.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255);
        });

        builder.Property(v => v.Phone)
            .HasMaxLength(50);

        builder.Property(v => v.Fax)
            .HasMaxLength(50);

        // Address fields (flattened value object)
        builder.Property(v => v.AddressStreet)
            .HasMaxLength(200);

        builder.Property(v => v.AddressStreet2)
            .HasMaxLength(200);

        builder.Property(v => v.AddressCity)
            .HasMaxLength(100);

        builder.Property(v => v.AddressStateProvince)
            .HasMaxLength(100);

        builder.Property(v => v.AddressPostalCode)
            .HasMaxLength(20);

        builder.Property(v => v.AddressCountry)
            .HasMaxLength(100);

        // Payment terms
        builder.Property(v => v.PaymentTerms)
            .HasMaxLength(200);

        builder.Property(v => v.PaymentDueDays);

        builder.Property(v => v.PreferredPaymentMethod)
            .HasMaxLength(50);

        builder.Property(v => v.AccountNumber)
            .HasMaxLength(50);

        builder.Property(v => v.CreditLimit)
            .HasPrecision(18, 2);

        builder.Property(v => v.Notes)
            .HasMaxLength(4000);

        builder.Property(v => v.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(v => v.TenantId)
            .IsRequired();

        builder.Property(v => v.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(v => v.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(v => v.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(v => new { v.TenantId, v.VendorNumber })
            .IsUnique()
            .HasDatabaseName("IX_Vendors_TenantId_VendorNumber");

        builder.HasIndex(v => v.Status)
            .HasDatabaseName("IX_Vendors_Status");

        builder.HasIndex(v => v.VendorType)
            .HasDatabaseName("IX_Vendors_VendorType");

        builder.HasIndex(v => new { v.TenantId, v.Name })
            .HasDatabaseName("IX_Vendors_TenantId_Name");

        builder.HasIndex(v => new { v.TenantId, v.TaxId })
            .HasDatabaseName("IX_Vendors_TenantId_TaxId");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
