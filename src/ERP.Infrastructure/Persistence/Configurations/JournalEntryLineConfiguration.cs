using ERP.Domain.FIN.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines", "fin");

        builder.HasKey(jel => jel.Id);

        builder.Property(jel => jel.Id)
            .HasColumnName("JournalEntryLineId")
            .ValueGeneratedOnAdd();

        builder.Property(jel => jel.JournalEntryId)
            .IsRequired();

        builder.Property(jel => jel.AccountId)
            .IsRequired();

        builder.Property(jel => jel.DebitAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(jel => jel.CreditAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(jel => jel.Description)
            .HasMaxLength(500)
            .IsRequired();

        // Audit fields
        builder.Property(jel => jel.TenantId)
            .IsRequired();

        builder.Property(jel => jel.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(jel => jel.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(jel => jel.RowVersion)
            .IsRowVersion();

        // Foreign key to Account
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(jel => jel.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(jel => jel.JournalEntryId)
            .HasDatabaseName("IX_JournalEntryLines_JournalEntryId");

        builder.HasIndex(jel => jel.AccountId)
            .HasDatabaseName("IX_JournalEntryLines_AccountId");
    }
}
