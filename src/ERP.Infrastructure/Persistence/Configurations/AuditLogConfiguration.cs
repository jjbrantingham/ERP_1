using ERP.Domain.AUDIT.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", "audit");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("AuditLogId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.EventType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(a => a.Severity)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.UserId);

        builder.Property(a => a.Username)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.TableName)
            .HasMaxLength(100);

        builder.Property(a => a.PrimaryKey)
            .HasMaxLength(50);

        // Audit fields
        builder.Property(a => a.TenantId)
            .IsRequired();

        builder.Property(a => a.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        // Indexes for common queries
        builder.HasIndex(a => new { a.TenantId, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_TenantId_Timestamp");

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("IX_AuditLogs_Entity");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        builder.HasIndex(a => a.EventType)
            .HasDatabaseName("IX_AuditLogs_EventType");

        builder.HasIndex(a => a.Severity)
            .HasDatabaseName("IX_AuditLogs_Severity");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        // Composite index for common search patterns
        builder.HasIndex(a => new { a.TenantId, a.EventType, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_Search");
    }
}
