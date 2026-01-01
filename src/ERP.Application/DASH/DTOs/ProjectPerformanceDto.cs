using ERP.Domain.PM.Enums;

namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Project performance summary for dashboards
/// </summary>
public class ProjectPerformanceDto
{
    public long ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public ProjectType Type { get; set; }

    // Financial metrics
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal BudgetVariance { get; set; }
    public decimal BudgetUtilizationPercent { get; set; }

    // Time metrics
    public decimal PlannedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal HoursVariance { get; set; }

    // Risk indicators
    public bool IsOverBudget { get; set; }
    public bool IsAtRisk { get; set; }
    public int DaysUntilDeadline { get; set; }

    public string Currency { get; set; } = "USD";
}
