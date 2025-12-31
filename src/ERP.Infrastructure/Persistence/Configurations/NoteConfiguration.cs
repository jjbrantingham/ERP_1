using ERP.Domain.CRM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Note entity.
/// </summary>
public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes", "crm");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("NoteId")
            .ValueGeneratedOnAdd();

        builder.Property(n => n.ClientId)
            .IsRequired();

        builder.Property(n => n.ContactId);

        builder.Property(n => n.NoteType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(n => n.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(n => n.NoteDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(n => n.FollowUpDate)
            .HasColumnType("date");

        builder.Property(n => n.IsFollowUpComplete)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.AuthorId);

        // Relationships
        builder.HasOne(n => n.Client)
            .WithMany(c => c.Notes)
            .HasForeignKey(n => n.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Contact)
            .WithMany()
            .HasForeignKey(n => n.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(n => n.ClientId);
        builder.HasIndex(n => n.ContactId);
        builder.HasIndex(n => n.NoteType);
        builder.HasIndex(n => n.NoteDate);
        builder.HasIndex(n => n.FollowUpDate);
        builder.HasIndex(n => n.AuthorId);

        builder.HasIndex(n => new { n.IsFollowUpComplete, n.FollowUpDate });

        // Ignore domain events
        builder.Ignore(n => n.DomainEvents);

        // Row version for optimistic concurrency
        builder.Property(n => n.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(n => n.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
