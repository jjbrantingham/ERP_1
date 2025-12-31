namespace ERP.Application.PM.DTOs;

/// <summary>
/// Data transfer object for WBS Item.
/// </summary>
public class WBSItemDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long ProjectId { get; set; }
    public long? ParentId { get; set; }
    public string WBSCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public decimal PercentComplete { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string? BudgetCurrency { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
