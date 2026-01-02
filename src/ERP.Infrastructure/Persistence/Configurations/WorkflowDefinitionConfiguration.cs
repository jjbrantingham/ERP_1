using ERP.Domain.WF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions", "wf");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("WorkflowDefinitionId")
            .ValueGeneratedOnAdd();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(2000);

        builder.Property(w => w.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(w => w.Version)
            .IsRequired();

        // Collections
        builder.HasMany(w => w.Steps)
            .WithOne()
            .HasForeignKey("WorkflowDefinitionId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => new { w.TenantId, w.EntityType, w.Status });
        builder.HasIndex(w => w.Status);

        // Row version for optimistic concurrency
        builder.Property(w => w.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(w => w.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
