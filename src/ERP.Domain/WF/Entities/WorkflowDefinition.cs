using ERP.Domain.Common;
using ERP.Domain.WF.Enums;
using ERP.Domain.WF.Events;

namespace ERP.Domain.WF.Entities;

/// <summary>
/// Defines a reusable workflow template
/// </summary>
public class WorkflowDefinition : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string EntityType { get; private set; } // e.g., "Timesheet", "ExpenseReport", "Invoice"
    public WorkflowDefinitionStatus Status { get; private set; }
    public int Version { get; private set; }

    private readonly List<WorkflowStep> _steps;
    public IReadOnlyCollection<WorkflowStep> Steps => _steps.AsReadOnly();

    private WorkflowDefinition()
    {
        Name = null!;
        Description = null!;
        EntityType = null!;
        _steps = new List<WorkflowStep>();
    }

    /// <summary>
    /// Create a new workflow definition
    /// </summary>
    public static WorkflowDefinition Create(
        Guid tenantId,
        string name,
        string description,
        string entityType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workflow name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));

        var workflow = new WorkflowDefinition
        {
            TenantId = tenantId,
            Name = name,
            Description = description ?? string.Empty,
            EntityType = entityType,
            Status = WorkflowDefinitionStatus.Draft,
            Version = 1
        };

        workflow.AddDomainEvent(new WorkflowDefinitionCreatedEvent(workflow.Id, tenantId, name, entityType));

        return workflow;
    }

    /// <summary>
    /// Add a step to the workflow
    /// </summary>
    public void AddStep(
        string name,
        string description,
        StepType stepType,
        int sequenceNumber,
        string? approverRole = null,
        long? approverId = null,
        int? timeoutHours = null)
    {
        if (Status != WorkflowDefinitionStatus.Draft)
            throw new InvalidOperationException("Cannot modify non-draft workflows");

        if (stepType == StepType.Approval && string.IsNullOrWhiteSpace(approverRole) && !approverId.HasValue)
            throw new ArgumentException("Approval steps require either approver role or approver ID");

        var step = WorkflowStep.Create(
            name,
            description,
            stepType,
            sequenceNumber,
            approverRole,
            approverId,
            timeoutHours);

        _steps.Add(step);
    }

    /// <summary>
    /// Activate the workflow definition
    /// </summary>
    public void Activate()
    {
        if (Status == WorkflowDefinitionStatus.Active)
            throw new InvalidOperationException("Workflow is already active");

        if (_steps.Count == 0)
            throw new InvalidOperationException("Cannot activate workflow with no steps");

        Status = WorkflowDefinitionStatus.Active;

        AddDomainEvent(new WorkflowDefinitionActivatedEvent(Id, TenantId, Name));
    }

    /// <summary>
    /// Deactivate the workflow definition
    /// </summary>
    public void Deactivate()
    {
        if (Status != WorkflowDefinitionStatus.Active)
            throw new InvalidOperationException("Only active workflows can be deactivated");

        Status = WorkflowDefinitionStatus.Inactive;

        AddDomainEvent(new WorkflowDefinitionDeactivatedEvent(Id, TenantId, Name));
    }

    /// <summary>
    /// Create a new version of this workflow
    /// </summary>
    public WorkflowDefinition CreateNewVersion()
    {
        var newWorkflow = new WorkflowDefinition
        {
            TenantId = TenantId,
            Name = Name,
            Description = Description,
            EntityType = EntityType,
            Status = WorkflowDefinitionStatus.Draft,
            Version = Version + 1
        };

        // Copy steps
        foreach (var step in _steps)
        {
            newWorkflow.AddStep(
                step.Name,
                step.Description,
                step.StepType,
                step.SequenceNumber,
                step.ApproverRole,
                step.ApproverId,
                step.TimeoutHours);
        }

        return newWorkflow;
    }
}
