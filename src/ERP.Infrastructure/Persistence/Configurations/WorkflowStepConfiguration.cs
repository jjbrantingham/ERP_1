using ERP.Domain.WF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("WorkflowSteps", "wf");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("WorkflowStepId")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.WorkflowDefinitionId)
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

        builder.Property(s => s.ApproverRole)
            .HasMaxLength(100);

        builder.Property(s => s.ApproverId);

        builder.Property(s => s.TimeoutHours);

        builder.Property(s => s.Conditions)
            .HasColumnType("nvarchar(max)");

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
        builder.HasIndex(s => new { s.WorkflowDefinitionId, s.SequenceNumber });
    }
}
