using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Contract entity.
/// </summary>
public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts", "pm");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("ContractId")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.ProjectId)
            .IsRequired();

        builder.Property(c => c.ContractNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ContractType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(c => c.Title)
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(4000);

        // Value object: Money (ContractValue)
        builder.OwnsOne(c => c.ContractValue, cv =>
        {
            cv.Property(m => m.Amount)
                .HasColumnName("ContractValueAmount")
                .HasPrecision(18, 2);
            cv.Property(m => m.Currency)
                .HasColumnName("ContractValueCurrency")
                .HasMaxLength(3);
        });

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();

        builder.Property(c => c.SignedDate);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Terms)
            .HasMaxLength(int.MaxValue); // Use MAX for large text

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
        builder.HasOne(c => c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.ContractNumber })
            .IsUnique()
            .HasDatabaseName("IX_Contracts_TenantId_ContractNumber");

        builder.HasIndex(c => c.ProjectId)
            .HasDatabaseName("IX_Contracts_ProjectId");

        builder.HasIndex(c => c.ContractType)
            .HasDatabaseName("IX_Contracts_ContractType");

        builder.HasIndex(c => new { c.IsActive, c.EndDate })
            .HasDatabaseName("IX_Contracts_IsActive_EndDate");

        // Global query filter for multi-tenancy
        // Note: This will be set in DbContext OnModelCreating using dynamic expression
    }
}
