using ERP.Domain.Common;
using ERP.Domain.WF.Enums;
using ERP.Domain.WF.Events;

namespace ERP.Domain.WF.Entities;

/// <summary>
/// Represents a runtime instance of a workflow attached to a specific entity
/// </summary>
public class WorkflowInstance : AggregateRoot
{
    public long WorkflowDefinitionId { get; private set; }
    public string EntityType { get; private set; }
    public long EntityId { get; private set; }
    public WorkflowInstanceStatus Status { get; private set; }
    public DateTime StartedDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string? CompletionReason { get; private set; }

    private readonly List<StepInstance> _stepInstances;
    public IReadOnlyCollection<StepInstance> StepInstances => _stepInstances.AsReadOnly();

    private WorkflowInstance()
    {
        EntityType = null!;
        _stepInstances = new List<StepInstance>();
    }

    /// <summary>
    /// Start a new workflow instance
    /// </summary>
    public static WorkflowInstance Start(
        Guid tenantId,
        long workflowDefinitionId,
        string entityType,
        long entityId,
        IEnumerable<WorkflowStep> steps)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));

        if (!steps.Any())
            throw new ArgumentException("Workflow must have at least one step", nameof(steps));

        var instance = new WorkflowInstance
        {
            TenantId = tenantId,
            WorkflowDefinitionId = workflowDefinitionId,
            EntityType = entityType,
            EntityId = entityId,
            Status = WorkflowInstanceStatus.Pending,
            StartedDate = DateTime.UtcNow
        };

        // Create step instances from workflow definition steps
        foreach (var step in steps.OrderBy(s => s.SequenceNumber))
        {
            var stepInstance = StepInstance.Create(
                step.Name,
                step.Description,
                step.StepType,
                step.SequenceNumber,
                step.ApproverRole,
                step.ApproverId,
                step.TimeoutHours);

            instance._stepInstances.Add(stepInstance);
        }

        instance.AddDomainEvent(new WorkflowInstanceStartedEvent(
            instance.Id,
            tenantId,
            workflowDefinitionId,
            entityType,
            entityId));

        return instance;
    }

    /// <summary>
    /// Begin workflow execution
    /// </summary>
    public void Begin()
    {
        if (Status != WorkflowInstanceStatus.Pending)
            throw new InvalidOperationException("Only pending workflows can be started");

        Status = WorkflowInstanceStatus.InProgress;

        // Activate the first step
        var firstStep = _stepInstances.OrderBy(s => s.SequenceNumber).FirstOrDefault();
        if (firstStep != null)
        {
            firstStep.Activate();
        }

        AddDomainEvent(new WorkflowInstanceProgressedEvent(Id, TenantId, EntityType, EntityId, Status));
    }

    /// <summary>
    /// Process an approval action on a step
    /// </summary>
    public void ProcessStepAction(
        int sequenceNumber,
        ApprovalAction action,
        long approverId,
        string approverName,
        string? comments = null)
    {
        if (Status != WorkflowInstanceStatus.InProgress)
            throw new InvalidOperationException("Cannot process actions on non-active workflows");

        var step = _stepInstances.FirstOrDefault(s => s.SequenceNumber == sequenceNumber);
        if (step == null)
            throw new ArgumentException($"Step {sequenceNumber} not found", nameof(sequenceNumber));

        if (step.Status != StepInstanceStatus.Active)
            throw new InvalidOperationException($"Step {sequenceNumber} is not active");

        // Process the action
        step.ProcessAction(action, approverId, approverName, comments);

        // Handle based on action
        switch (action)
        {
            case ApprovalAction.Approved:
                HandleStepApproved(step);
                break;

            case ApprovalAction.Rejected:
                HandleStepRejected(step);
                break;

            case ApprovalAction.ReturnedForRevision:
                HandleStepReturned(step);
                break;

            default:
                throw new NotSupportedException($"Action {action} not supported");
        }
    }

    private void HandleStepApproved(StepInstance step)
    {
        // Check if there are more steps
        var nextStep = _stepInstances
            .Where(s => s.SequenceNumber > step.SequenceNumber)
            .OrderBy(s => s.SequenceNumber)
            .FirstOrDefault();

        if (nextStep != null)
        {
            // Activate next step
            nextStep.Activate();
            AddDomainEvent(new WorkflowStepActivatedEvent(Id, TenantId, nextStep.SequenceNumber, nextStep.Name));
        }
        else
        {
            // Workflow completed successfully
            Complete("All steps approved");
        }
    }

    private void HandleStepRejected(StepInstance step)
    {
        Status = WorkflowInstanceStatus.Rejected;
        CompletedDate = DateTime.UtcNow;
        CompletionReason = $"Rejected at step {step.SequenceNumber}: {step.Name}";

        AddDomainEvent(new WorkflowInstanceRejectedEvent(
            Id,
            TenantId,
            EntityType,
            EntityId,
            step.SequenceNumber,
            step.Name,
            step.CompletedByUserId ?? 0, // Default to 0 if not set (system user)
            step.Comments));
    }

    private void HandleStepReturned(StepInstance step)
    {
        // Keep workflow in progress, entity needs revision
        AddDomainEvent(new WorkflowStepReturnedEvent(
            Id,
            TenantId,
            EntityType,
            EntityId,
            step.SequenceNumber,
            step.Name));
    }

    /// <summary>
    /// Complete the workflow successfully
    /// </summary>
    public void Complete(string reason)
    {
        if (Status == WorkflowInstanceStatus.Completed)
            throw new InvalidOperationException("Workflow is already completed");

        Status = WorkflowInstanceStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        CompletionReason = reason;

        AddDomainEvent(new WorkflowInstanceCompletedEvent(
            Id,
            TenantId,
            EntityType,
            EntityId,
            CompletedDate.Value));
    }

    /// <summary>
    /// Cancel the workflow
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == WorkflowInstanceStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed workflow");

        if (Status == WorkflowInstanceStatus.Cancelled)
            throw new InvalidOperationException("Workflow is already cancelled");

        Status = WorkflowInstanceStatus.Cancelled;
        CompletedDate = DateTime.UtcNow;
        CompletionReason = reason;

        AddDomainEvent(new WorkflowInstanceCancelledEvent(
            Id,
            TenantId,
            EntityType,
            EntityId,
            reason));
    }

    /// <summary>
    /// Handle timeout escalation for a step
    /// </summary>
    public void HandleStepTimeout(int sequenceNumber)
    {
        var step = _stepInstances.FirstOrDefault(s => s.SequenceNumber == sequenceNumber);
        if (step == null)
            throw new ArgumentException($"Step {sequenceNumber} not found", nameof(sequenceNumber));

        step.Timeout();

        AddDomainEvent(new WorkflowStepTimedOutEvent(
            Id,
            TenantId,
            EntityType,
            EntityId,
            sequenceNumber,
            step.Name));
    }

    /// <summary>
    /// Get the current active step
    /// </summary>
    public StepInstance? GetCurrentStep()
    {
        return _stepInstances.FirstOrDefault(s => s.Status == StepInstanceStatus.Active);
    }

    /// <summary>
    /// Check if workflow is in a terminal state
    /// </summary>
    public bool IsTerminal()
    {
        return Status is WorkflowInstanceStatus.Completed
            or WorkflowInstanceStatus.Rejected
            or WorkflowInstanceStatus.Cancelled
            or WorkflowInstanceStatus.Error;
    }
}
