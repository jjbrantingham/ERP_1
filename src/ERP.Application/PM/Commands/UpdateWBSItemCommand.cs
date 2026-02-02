using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to update an existing WBS item.
/// </summary>
public class UpdateWBSItemCommand : IRequest<Unit>
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public decimal? EstimatedHours { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
