using ERP.Domain.HR.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Rate entity.
/// </summary>
public class RateConfiguration : IEntityTypeConfiguration<Rate>
{
    public void Configure(EntityTypeBuilder<Rate> builder)
    {
        builder.ToTable("Rates", "hr");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("RateId")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.EmployeeId);
        builder.Property(r => r.ResourceTypeId);

        builder.Property(r => r.RateType)
            .IsRequired()
            .HasConversion<byte>();

        // Cost rate value object
        builder.OwnsOne(r => r.CostRate, cost =>
        {
            cost.Property(m => m.Amount)
                .HasColumnName("CostRateAmount")
                .IsRequired()
                .HasPrecision(18, 2);

            cost.Property(m => m.Currency)
                .HasColumnName("CostRateCurrency")
                .IsRequired()
                .HasMaxLength(3);
        });

        // Billing rate value object
        builder.OwnsOne(r => r.BillingRate, billing =>
        {
            billing.Property(m => m.Amount)
                .HasColumnName("BillingRateAmount")
                .IsRequired()
                .HasPrecision(18, 2);

            billing.Property(m => m.Currency)
                .HasColumnName("BillingRateCurrency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Property(r => r.EffectiveDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(r => r.EndDate)
            .HasColumnType("date");

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(r => r.Employee)
            .WithMany(e => e.Rates)
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ResourceType)
            .WithMany(rt => rt.Rates)
            .HasForeignKey(r => r.ResourceTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.EmployeeId);
        builder.HasIndex(r => r.ResourceTypeId);
        builder.HasIndex(r => r.RateType);
        builder.HasIndex(r => r.EffectiveDate);
        builder.HasIndex(r => r.IsActive);

        builder.HasIndex(r => new { r.EmployeeId, r.RateType, r.EffectiveDate });
        builder.HasIndex(r => new { r.ResourceTypeId, r.RateType, r.EffectiveDate });

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);

        // Row version for optimistic concurrency
        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(r => r.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
