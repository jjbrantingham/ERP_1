using ERP.Domain.Common;
using ERP.Domain.WF.Enums;

namespace ERP.Domain.WF.Entities;

/// <summary>
/// Represents a step in a workflow definition
/// </summary>
public class WorkflowStep : Entity
{
    public long WorkflowDefinitionId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public StepType StepType { get; private set; }
    public int SequenceNumber { get; private set; }

    /// <summary>
    /// Role required to approve this step (e.g., "Manager", "Finance Director")
    /// </summary>
    public string? ApproverRole { get; private set; }

    /// <summary>
    /// Specific user ID required to approve (optional)
    /// </summary>
    public long? ApproverId { get; private set; }

    /// <summary>
    /// Timeout in hours before escalation
    /// </summary>
    public int? TimeoutHours { get; private set; }

    /// <summary>
    /// Conditions for conditional steps (JSON)
    /// </summary>
    public string? Conditions { get; private set; }

    private WorkflowStep()
    {
        Name = null!;
        Description = null!;
    }

    /// <summary>
    /// Create a new workflow step
    /// </summary>
    public static WorkflowStep Create(
        string name,
        string description,
        StepType stepType,
        int sequenceNumber,
        string? approverRole = null,
        long? approverId = null,
        int? timeoutHours = null,
        string? conditions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Step name is required", nameof(name));

        if (sequenceNumber < 1)
            throw new ArgumentException("Sequence number must be greater than zero", nameof(sequenceNumber));

        return new WorkflowStep
        {
            Name = name,
            Description = description ?? string.Empty,
            StepType = stepType,
            SequenceNumber = sequenceNumber,
            ApproverRole = approverRole,
            ApproverId = approverId,
            TimeoutHours = timeoutHours,
            Conditions = conditions
        };
    }
}
