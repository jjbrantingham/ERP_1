using ERP.Domain.WF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances", "wf");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("WorkflowInstanceId")
            .ValueGeneratedOnAdd();

        builder.Property(w => w.WorkflowDefinitionId)
            .IsRequired();

        builder.Property(w => w.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.EntityId)
            .IsRequired();

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(w => w.StartedDate)
            .IsRequired();

        builder.Property(w => w.CompletedDate);

        builder.Property(w => w.CompletionReason)
            .HasMaxLength(1000);

        // Collections
        builder.HasMany(w => w.StepInstances)
            .WithOne()
            .HasForeignKey("WorkflowInstanceId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => new { w.TenantId, w.EntityType, w.EntityId });
        builder.HasIndex(w => w.Status);
        builder.HasIndex(w => w.WorkflowDefinitionId);

        // Row version for optimistic concurrency
        builder.Property(w => w.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(w => w.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
