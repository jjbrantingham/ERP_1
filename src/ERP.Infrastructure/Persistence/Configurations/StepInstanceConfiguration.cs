using ERP.Domain.WF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class StepInstanceConfiguration : IEntityTypeConfiguration<StepInstance>
{
    public void Configure(EntityTypeBuilder<StepInstance> builder)
    {
        builder.ToTable("StepInstances", "wf");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("StepInstanceId")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.WorkflowInstanceId)
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.StepType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(s => s.SequenceNumber)
            .IsRequired();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(s => s.ApproverRole)
            .HasMaxLength(100);

        builder.Property(s => s.ApproverId);

        builder.Property(s => s.TimeoutHours);

        builder.Property(s => s.ActivatedDate);

        builder.Property(s => s.CompletedDate);

        builder.Property(s => s.DeadlineDate);

        builder.Property(s => s.CompletedByUserId);

        builder.Property(s => s.CompletedByUserName)
            .HasMaxLength(200);

        builder.Property(s => s.Action)
            .HasConversion<byte>();

        builder.Property(s => s.Comments)
            .HasMaxLength(2000);

        // Audit fields
        builder.Property(s => s.TenantId)
            .IsRequired();

        builder.Property(s => s.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(s => s.ModifiedDate);

        // Row version for optimistic concurrency
        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(s => new { s.WorkflowInstanceId, s.SequenceNumber });
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => new { s.Status, s.DeadlineDate });
    }
}
