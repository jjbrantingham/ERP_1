using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries", "fin");

        builder.HasKey(je => je.Id);

        builder.Property(je => je.Id)
            .HasColumnName("JournalEntryId")
            .ValueGeneratedOnAdd();

        builder.Property(je => je.TenantId)
            .IsRequired();

        // Value object mapping for JournalEntryNumber
        builder.Property(je => je.EntryNumber)
            .HasConversion(
                v => v.Value,
                v => new JournalEntryNumber(v))
            .HasColumnName("EntryNumber")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(je => je.EntryDate)
            .IsRequired();

        builder.Property(je => je.Type)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(je => je.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(je => je.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(je => je.Reference)
            .HasMaxLength(100);

        builder.Property(je => je.FiscalPeriod)
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(je => je.PostedDate);

        builder.Property(je => je.PostedBy)
            .HasMaxLength(100);

        builder.Property(je => je.VoidedDate);

        builder.Property(je => je.VoidedBy)
            .HasMaxLength(100);

        builder.Property(je => je.VoidReason)
            .HasMaxLength(500);

        // Audit fields
        builder.Property(je => je.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(je => je.ModifiedDate);

        builder.Property(je => je.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasMany(je => je.Lines)
            .WithOne()
            .HasForeignKey("JournalEntryId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(je => je.TenantId)
            .HasDatabaseName("IX_JournalEntries_TenantId");

        builder.HasIndex(je => new { je.TenantId, je.EntryNumber })
            .IsUnique()
            .HasDatabaseName("IX_JournalEntries_TenantId_EntryNumber");

        builder.HasIndex(je => je.Status)
            .HasDatabaseName("IX_JournalEntries_Status");

        builder.HasIndex(je => je.EntryDate)
            .HasDatabaseName("IX_JournalEntries_EntryDate");

        builder.HasIndex(je => je.FiscalPeriod)
            .HasDatabaseName("IX_JournalEntries_FiscalPeriod");
    }
}
