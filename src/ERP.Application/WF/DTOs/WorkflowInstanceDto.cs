using ERP.Domain.WF.Enums;

namespace ERP.Application.WF.DTOs;

public class WorkflowInstanceDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long WorkflowDefinitionId { get; set; }
    public string WorkflowDefinitionName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public WorkflowInstanceStatus Status { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CompletionReason { get; set; }
    public List<StepInstanceDto> Steps { get; set; } = new();
    public StepInstanceDto? CurrentStep { get; set; }
}

public class StepInstanceDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StepType StepType { get; set; }
    public int SequenceNumber { get; set; }
    public StepInstanceStatus Status { get; set; }
    public string? ApproverRole { get; set; }
    public long? ApproverId { get; set; }
    public int? TimeoutHours { get; set; }
    public DateTime? ActivatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? DeadlineDate { get; set; }
    public long? CompletedByUserId { get; set; }
    public string? CompletedByUserName { get; set; }
    public ApprovalAction? Action { get; set; }
    public string? Comments { get; set; }
    public bool IsOverdue { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
}
