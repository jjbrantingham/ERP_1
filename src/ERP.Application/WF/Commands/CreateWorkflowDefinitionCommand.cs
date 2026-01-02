using MediatR;

namespace ERP.Application.WF.Commands;

public class CreateWorkflowDefinitionCommand : IRequest<long>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public List<WorkflowStepCommand> Steps { get; init; } = new();
}

public class WorkflowStepCommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public byte StepType { get; init; }
    public int SequenceNumber { get; init; }
    public string? ApproverRole { get; init; }
    public long? ApproverId { get; init; }
    public int? TimeoutHours { get; init; }
    public string? Conditions { get; init; }
}
