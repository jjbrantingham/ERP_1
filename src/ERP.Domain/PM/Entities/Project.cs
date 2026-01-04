using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.ValueObjects;

namespace ERP.Domain.PM.Entities;

/// <summary>
/// Represents a project.
/// Main aggregate root for the Project Management module.
/// </summary>
public class Project : AggregateRoot
{
    public ProjectNumber ProjectNumber { get; private set; }
    public long ClientId { get; private set; }
    public ProjectType ProjectType { get; private set; }
    public ProjectStatus Status { get; private set; }
    public BillingMode BillingMode { get; private set; }

    public string Name { get; private set; }
    public string? Description { get; private set; }

    // Dates
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }

    // Budget
    public Money? Budget { get; private set; }
    public Money? ActualCost { get; private set; }

    // Management
    public long? ProjectManagerId { get; private set; }

    // Additional
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<WBSItem> _wbsItems = new();
    public IReadOnlyCollection<WBSItem> WBSItems => _wbsItems.AsReadOnly();

    private readonly List<Contract> _contracts = new();
    public IReadOnlyCollection<Contract> Contracts => _contracts.AsReadOnly();

    private readonly List<ResourceAllocation> _resourceAllocations = new();
    public IReadOnlyCollection<ResourceAllocation> ResourceAllocations => _resourceAllocations.AsReadOnly();

    private Project()
    {
        ProjectNumber = new ProjectNumber("TEMP");
        Name = string.Empty;
    }

    public static Project Create(
        Guid tenantId,
        ProjectNumber projectNumber,
        long clientId,
        string name,
        ProjectType projectType,
        BillingMode billingMode,
        DateTime startDate,
        string? description = null,
        DateTime? endDate = null,
        Money? budget = null,
        long? projectManagerId = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required", nameof(name));

        var project = new Project
        {
            TenantId = tenantId,
            ProjectNumber = projectNumber,
            ClientId = clientId,
            Name = name.Trim(),
            Description = description?.Trim(),
            ProjectType = projectType,
            BillingMode = billingMode,
            Status = ProjectStatus.Planning,
            StartDate = startDate,
            EndDate = endDate,
            Budget = budget,
            ProjectManagerId = projectManagerId,
            Notes = notes?.Trim(),
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, project.ProjectNumber, project.Name));

        return project;
    }

    public void UpdateInfo(
        string name,
        ProjectType projectType,
        BillingMode billingMode,
        DateTime startDate,
        string? description = null,
        DateTime? endDate = null,
        Money? budget = null,
        long? projectManagerId = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        ProjectType = projectType;
        BillingMode = billingMode;
        StartDate = startDate;
        EndDate = endDate;
        Budget = budget;
        ProjectManagerId = projectManagerId;
        Notes = notes?.Trim();
        ModifiedDate = DateTime.UtcNow;
    }

    public void ChangeStatus(ProjectStatus newStatus)
    {
        var oldStatus = Status;
        Status = newStatus;
        ModifiedDate = DateTime.UtcNow;

        if (newStatus == ProjectStatus.Active && !ActualStartDate.HasValue)
        {
            ActualStartDate = DateTime.UtcNow;
        }

        if ((newStatus == ProjectStatus.Completed || newStatus == ProjectStatus.Closed) && !ActualEndDate.HasValue)
        {
            ActualEndDate = DateTime.UtcNow;
        }

        AddDomainEvent(new ProjectStatusChangedEvent(Id, ProjectNumber, Name, oldStatus, newStatus));
    }

    public void Activate()
    {
        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdateActualCost(Money actualCost)
    {
        ActualCost = actualCost;
        ModifiedDate = DateTime.UtcNow;
    }
}
