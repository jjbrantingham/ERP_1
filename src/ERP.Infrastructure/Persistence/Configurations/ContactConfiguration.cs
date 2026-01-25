using ERP.Domain.CRM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Contact entity.
/// </summary>
public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts", "crm");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("ContactId")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.ClientId)
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

        builder.Property(c => c.MiddleName)
            .HasMaxLength(100);

        builder.Property(c => c.JobTitle)
            .HasMaxLength(200);

        builder.Property(c => c.Department)
            .HasMaxLength(200);

        // Email value object
        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);

            email.HasIndex(e => e.Value)
                .HasDatabaseName("IX_Contacts_Email");
        });

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(c => c.MobileNumber)
            .HasMaxLength(20);

        builder.Property(c => c.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Notes)
            .HasMaxLength(4000);

        // Relationships
        builder.HasOne(c => c.Client)
            .WithMany(cl => cl.Contacts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.ClientId);
        builder.HasIndex(c => c.ContactType);
        builder.HasIndex(c => c.IsPrimary);
        builder.HasIndex(c => c.IsActive);

        builder.HasIndex(c => new { c.FirstName, c.LastName });

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
