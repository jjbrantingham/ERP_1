using ERP.Domain.Common;
using ERP.Domain.WF.Enums;

namespace ERP.Domain.WF.Entities;

/// <summary>
/// Represents a runtime instance of a workflow step
/// </summary>
public class StepInstance : Entity
{
    public long WorkflowInstanceId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public StepType StepType { get; private set; }
    public int SequenceNumber { get; private set; }
    public StepInstanceStatus Status { get; private set; }

    /// <summary>
    /// Role required to approve this step
    /// </summary>
    public string? ApproverRole { get; private set; }

    /// <summary>
    /// Specific user ID required to approve
    /// </summary>
    public long? ApproverId { get; private set; }

    /// <summary>
    /// Timeout in hours before escalation
    /// </summary>
    public int? TimeoutHours { get; private set; }

    /// <summary>
    /// When the step was activated
    /// </summary>
    public DateTime? ActivatedDate { get; private set; }

    /// <summary>
    /// When the step was completed
    /// </summary>
    public DateTime? CompletedDate { get; private set; }

    /// <summary>
    /// Deadline for completion (ActivatedDate + TimeoutHours)
    /// </summary>
    public DateTime? DeadlineDate { get; private set; }

    /// <summary>
    /// User who completed the step
    /// </summary>
    public long? CompletedByUserId { get; private set; }

    /// <summary>
    /// Name of user who completed the step
    /// </summary>
    public string? CompletedByUserName { get; private set; }

    /// <summary>
    /// Action taken (Approved, Rejected, etc.)
    /// </summary>
    public ApprovalAction? Action { get; private set; }

    /// <summary>
    /// Comments provided by approver
    /// </summary>
    public string? Comments { get; private set; }

    private StepInstance()
    {
        Name = null!;
        Description = null!;
    }

    /// <summary>
    /// Create a new step instance from a workflow step definition
    /// </summary>
    internal static StepInstance Create(
        string name,
        string description,
        StepType stepType,
        int sequenceNumber,
        string? approverRole = null,
        long? approverId = null,
        int? timeoutHours = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Step name is required", nameof(name));

        if (sequenceNumber < 1)
            throw new ArgumentException("Sequence number must be greater than zero", nameof(sequenceNumber));

        return new StepInstance
        {
            Name = name,
            Description = description ?? string.Empty,
            StepType = stepType,
            SequenceNumber = sequenceNumber,
            Status = StepInstanceStatus.Waiting,
            ApproverRole = approverRole,
            ApproverId = approverId,
            TimeoutHours = timeoutHours
        };
    }

    /// <summary>
    /// Activate this step for approval
    /// </summary>
    internal void Activate()
    {
        if (Status != StepInstanceStatus.Waiting)
            throw new InvalidOperationException("Only waiting steps can be activated");

        Status = StepInstanceStatus.Active;
        ActivatedDate = DateTime.UtcNow;

        // Calculate deadline if timeout is specified
        if (TimeoutHours.HasValue)
        {
            DeadlineDate = ActivatedDate.Value.AddHours(TimeoutHours.Value);
        }
    }

    /// <summary>
    /// Process an approval action
    /// </summary>
    internal void ProcessAction(
        ApprovalAction action,
        long userId,
        string userName,
        string? comments = null)
    {
        if (Status != StepInstanceStatus.Active)
            throw new InvalidOperationException("Only active steps can be processed");

        Action = action;
        CompletedByUserId = userId;
        CompletedByUserName = userName;
        Comments = comments;
        CompletedDate = DateTime.UtcNow;

        Status = action switch
        {
            ApprovalAction.Approved => StepInstanceStatus.Completed,
            ApprovalAction.Rejected => StepInstanceStatus.Rejected,
            ApprovalAction.ReturnedForRevision => StepInstanceStatus.Completed,
            _ => throw new NotSupportedException($"Action {action} not supported")
        };
    }

    /// <summary>
    /// Mark step as timed out
    /// </summary>
    internal void Timeout()
    {
        if (Status != StepInstanceStatus.Active)
            throw new InvalidOperationException("Only active steps can timeout");

        Status = StepInstanceStatus.TimedOut;
    }

    /// <summary>
    /// Skip this step (for conditional workflows)
    /// </summary>
    internal void Skip(string reason)
    {
        if (Status != StepInstanceStatus.Waiting)
            throw new InvalidOperationException("Only waiting steps can be skipped");

        Status = StepInstanceStatus.Skipped;
        Comments = reason;
    }

    /// <summary>
    /// Check if step is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return Status == StepInstanceStatus.Active
            && DeadlineDate.HasValue
            && DateTime.UtcNow > DeadlineDate.Value;
    }

    /// <summary>
    /// Get time remaining until deadline
    /// </summary>
    public TimeSpan? GetTimeRemaining()
    {
        if (Status != StepInstanceStatus.Active || !DeadlineDate.HasValue)
            return null;

        var remaining = DeadlineDate.Value - DateTime.UtcNow;
        return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
    }
}
