using ERP.Domain.VM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for VendorNote entity.
/// </summary>
public class VendorNoteConfiguration : IEntityTypeConfiguration<VendorNote>
{
    public void Configure(EntityTypeBuilder<VendorNote> builder)
    {
        builder.ToTable("VendorNotes", "vm");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("VendorNoteId")
            .ValueGeneratedOnAdd();

        builder.Property(n => n.VendorId)
            .IsRequired();

        builder.Property(n => n.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(int.MaxValue); // Use MAX for large text

        builder.Property(n => n.NoteDate)
            .IsRequired();

        builder.Property(n => n.CreatedByUserId);

        // Audit fields
        builder.Property(n => n.TenantId)
            .IsRequired();

        builder.Property(n => n.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(n => n.ModifiedDate);

        // Relationships
        builder.HasOne(n => n.Vendor)
            .WithMany()
            .HasForeignKey(n => n.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => n.VendorId)
            .HasDatabaseName("IX_VendorNotes_VendorId");

        builder.HasIndex(n => n.NoteDate)
            .HasDatabaseName("IX_VendorNotes_NoteDate");

        builder.HasIndex(n => new { n.VendorId, n.NoteDate })
            .HasDatabaseName("IX_VendorNotes_VendorId_NoteDate");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
