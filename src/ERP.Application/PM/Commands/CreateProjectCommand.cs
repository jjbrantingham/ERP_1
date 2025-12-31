using ERP.Domain.PM.Enums;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to create a new project.
/// </summary>
public class CreateProjectCommand
{
    public long ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectType ProjectType { get; set; }
    public BillingMode BillingMode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string? BudgetCurrency { get; set; }
    public long? ProjectManagerId { get; set; }
    public string? Notes { get; set; }
}
