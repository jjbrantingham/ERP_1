using ERP.Domain.Common;
using ERP.Domain.Common;

namespace ERP.Domain.PM.Entities;

/// <summary>
/// Work Breakdown Structure item for a project.
/// Represents hierarchical tasks/phases.
/// </summary>
public class WBSItem : Entity
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
}
