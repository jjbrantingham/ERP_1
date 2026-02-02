using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to create a new WBS item.
/// </summary>
public class CreateWBSItemCommand : IRequest<long>
{
    public long ProjectId { get; init; }
    public long? ParentId { get; init; }
    public string WBSCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public decimal? EstimatedHours { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
