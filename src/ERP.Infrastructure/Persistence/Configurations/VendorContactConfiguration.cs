using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for VendorContact entity.
/// </summary>
public class VendorContactConfiguration : IEntityTypeConfiguration<VendorContact>
{
    public void Configure(EntityTypeBuilder<VendorContact> builder)
    {
        builder.ToTable("VendorContacts", "vm");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("VendorContactId")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.VendorId)
            .IsRequired();

        builder.Property(c => c.ContactType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Title)
            .HasMaxLength(100);

        // Value object: Email
        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(255);
        });

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Mobile)
            .HasMaxLength(50);

        builder.Property(c => c.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.Notes)
            .HasMaxLength(4000);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit fields
        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(c => c.Vendor)
            .WithMany()
            .HasForeignKey(c => c.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.VendorId)
            .HasDatabaseName("IX_VendorContacts_VendorId");

        builder.HasIndex(c => c.ContactType)
            .HasDatabaseName("IX_VendorContacts_ContactType");

        builder.HasIndex(c => new { c.VendorId, c.IsPrimary })
            .HasDatabaseName("IX_VendorContacts_VendorId_IsPrimary");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
