namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to create a new WBS item.
/// </summary>
public class CreateWBSItemCommand
{
    public long ProjectId { get; set; }
    public long? ParentId { get; set; }
    public string WBSCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string? BudgetCurrency { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
