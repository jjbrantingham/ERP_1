using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts", "fin");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("AccountId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.TenantId)
            .IsRequired();

        // Value object mapping for AccountNumber
        builder.Property(a => a.AccountNumber)
            .HasConversion(
                v => v.Value,
                v => new AccountNumber(v))
            .HasColumnName("AccountNumber")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(a => a.ParentAccountId)
            .IsRequired(false);

        builder.Property(a => a.Balance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(a => a.AllowPosting)
            .IsRequired();

        builder.Property(a => a.RequiresReconciliation)
            .IsRequired();

        // Audit fields
        builder.Property(a => a.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.ModifiedDate);

        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("IX_Accounts_TenantId");

        builder.HasIndex(a => new { a.TenantId, a.AccountNumber })
            .IsUnique()
            .HasDatabaseName("IX_Accounts_TenantId_AccountNumber");

        builder.HasIndex(a => a.Type)
            .HasDatabaseName("IX_Accounts_Type");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_Accounts_Status");

        builder.HasIndex(a => a.ParentAccountId)
            .HasDatabaseName("IX_Accounts_ParentAccountId");
    }
}
