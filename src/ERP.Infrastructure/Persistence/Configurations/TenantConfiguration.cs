using ERP.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Tenant entity.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "common");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("TenantId")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.TenantGuid)
            .IsRequired();

        builder.Property(t => t.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.SubscriptionStartDate)
            .IsRequired();

        builder.Property(t => t.SubscriptionEndDate)
            .IsRequired(false);

        builder.Property(t => t.SubscriptionPlan)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Standard");

        builder.Property(t => t.MaxUsers)
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(t => t.Settings)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(t => t.DefaultCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(t => t.TimeZone)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("UTC");

        builder.Property(t => t.TenantId)
            .IsRequired();

        builder.Property(t => t.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.CreatedBy)
            .IsRequired(false);

        builder.Property(t => t.ModifiedDate)
            .IsRequired(false);

        builder.Property(t => t.ModifiedBy)
            .IsRequired(false);

        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(t => t.TenantGuid)
            .IsUnique();

        builder.HasIndex(t => t.Subdomain)
            .IsUnique();

        builder.HasIndex(t => t.IsActive);
    }
}
