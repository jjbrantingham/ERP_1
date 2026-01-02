using ERP.Domain.WF.Enums;

namespace ERP.Application.WF.DTOs;

public class WorkflowDefinitionDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public WorkflowDefinitionStatus Status { get; set; }
    public int Version { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
}

public class WorkflowStepDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StepType StepType { get; set; }
    public int SequenceNumber { get; set; }
    public string? ApproverRole { get; set; }
    public long? ApproverId { get; set; }
    public int? TimeoutHours { get; set; }
    public string? Conditions { get; set; }
}
