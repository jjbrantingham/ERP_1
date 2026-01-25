using ERP.Domain.CRM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Client entity.
/// </summary>
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients", "crm");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("ClientId")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.ClientNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ClientType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.LegalName)
            .HasMaxLength(200);

        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.Property(c => c.Website)
            .HasMaxLength(200);

        builder.Property(c => c.Industry)
            .HasMaxLength(100);

        // Primary email value object
        builder.OwnsOne(c => c.PrimaryEmail, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("PrimaryEmail")
                .HasMaxLength(256);
        });

        builder.Property(c => c.PrimaryPhone)
            .HasMaxLength(20);

        // Billing address value object
        builder.OwnsOne(c => c.BillingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("BillingStreet")
                .HasMaxLength(200);

            address.Property(a => a.Street2)
                .HasColumnName("BillingStreet2")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("BillingCity")
                .HasMaxLength(100);

            address.Property(a => a.StateProvince)
                .HasColumnName("BillingStateProvince")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("BillingPostalCode")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("BillingCountry")
                .HasMaxLength(100);
        });

        // Shipping address value object
        builder.OwnsOne(c => c.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .HasMaxLength(200);

            address.Property(a => a.Street2)
                .HasColumnName("ShippingStreet2")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .HasMaxLength(100);

            address.Property(a => a.StateProvince)
                .HasColumnName("ShippingStateProvince")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("ShippingPostalCode")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .HasMaxLength(100);
        });

        builder.Property(c => c.FirstContactDate)
            .HasColumnType("date");

        builder.Property(c => c.LastContactDate)
            .HasColumnType("date");

        builder.Property(c => c.AccountManagerId);

        builder.Property(c => c.PaymentTerms)
            .HasMaxLength(100);

        builder.Property(c => c.CreditLimit)
            .HasMaxLength(50);

        builder.Property(c => c.GeneralNotes)
            .HasMaxLength(4000);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasMany(c => c.Contacts)
            .WithOne(co => co.Client)
            .HasForeignKey(co => co.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notes relationship is configured in NoteConfiguration

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.ClientNumber })
            .IsUnique();

        builder.HasIndex(c => new { c.TenantId, c.Name });

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.ClientType);
        builder.HasIndex(c => c.Industry);
        builder.HasIndex(c => c.AccountManagerId);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.LastContactDate);

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);

        // Row version for optimistic concurrency
        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(c => c.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
