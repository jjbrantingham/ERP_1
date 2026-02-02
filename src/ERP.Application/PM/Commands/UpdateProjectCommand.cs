using ERP.Domain.PM.Enums;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to update an existing project.
/// </summary>
public class UpdateProjectCommand : IRequest
{
    public long ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectType ProjectType { get; init; }
    public BillingMode BillingMode { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
    public long? ProjectManagerId { get; init; }
    public string? Notes { get; init; }
}
