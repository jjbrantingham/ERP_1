namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Project profitability analysis
/// </summary>
public class ProjectProfitabilityDto
{
    public List<ProjectProfitabilityLineDto> Projects { get; set; } = new();
    public ProjectProfitabilitySummaryDto Summary { get; set; } = new();
}

public class ProjectProfitabilityLineDto
{
    public long ProjectId { get; set; }
    public string ProjectNumber { get; set; } = null!;
    public string ProjectName { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string BillingMode { get; set; } = null!;

    // Budget
    public decimal BudgetedHours { get; set; }
    public decimal BudgetedCost { get; set; }
    public decimal BudgetedRevenue { get; set; }

    // Actuals
    public decimal ActualHours { get; set; }
    public decimal ActualCost { get; set; }
    public decimal ActualRevenue { get; set; }

    // Variance
    public decimal HoursVariance { get; set; }
    public decimal CostVariance { get; set; }
    public decimal RevenueVariance { get; set; }

    // Profitability
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }

    // Utilization
    public decimal HoursUtilization { get; set; }
    public decimal CostUtilization { get; set; }
}

public class ProjectProfitabilitySummaryDto
{
    public int TotalProjects { get; set; }
    public decimal TotalBudgetedRevenue { get; set; }
    public decimal TotalActualRevenue { get; set; }
    public decimal TotalBudgetedCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal TotalGrossProfit { get; set; }
    public decimal OverallGrossProfitMargin { get; set; }
}
