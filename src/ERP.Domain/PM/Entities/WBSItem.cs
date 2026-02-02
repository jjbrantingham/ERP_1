using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;

namespace ERP.Domain.PM.Entities;

/// <summary>
/// Work Breakdown Structure item for a project.
/// Represents hierarchical tasks/phases.
/// </summary>
public class WBSItem : AggregateRoot
{
    public long ProjectId { get; private set; }
    public long? ParentId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int Level { get; private set; }
    public int DisplayOrder { get; private set; }
    public Money? Budget { get; private set; }
    public decimal? EstimatedHours { get; private set; }
    public decimal? ActualHours { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public decimal PercentComplete { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public Project? Project { get; private set; }
    public WBSItem? Parent { get; private set; }

    private WBSItem()
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    public static WBSItem Create(
        Guid tenantId,
        long projectId,
        string code,
        string name,
        int level,
        int displayOrder,
        long? parentId = null,
        string? description = null,
        Money? budget = null,
        decimal? estimatedHours = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return new WBSItem
        {
            TenantId = tenantId,
            ProjectId = projectId,
            ParentId = parentId,
            Code = code.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Level = level,
            DisplayOrder = displayOrder,
            Budget = budget,
            EstimatedHours = estimatedHours,
            StartDate = startDate,
            EndDate = endDate,
            PercentComplete = 0,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void UpdateProgress(decimal percentComplete, decimal? actualHours = null)
    {
        PercentComplete = Math.Clamp(percentComplete, 0, 100);
        if (actualHours.HasValue)
        {
            ActualHours = actualHours.Value;
        }
        ModifiedDate = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string? description,
        int displayOrder,
        Money? budget,
        decimal? estimatedHours,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (endDate.HasValue && startDate.HasValue && endDate.Value < startDate.Value)
            throw new ArgumentException("End date must be after start date", nameof(endDate));

        Name = name.Trim();
        Description = description?.Trim();
        DisplayOrder = displayOrder;
        Budget = budget;
        EstimatedHours = estimatedHours;
        StartDate = startDate;
        EndDate = endDate;
        ModifiedDate = DateTime.UtcNow;
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
}
