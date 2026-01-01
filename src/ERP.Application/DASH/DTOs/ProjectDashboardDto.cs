namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Project Manager dashboard with project-specific metrics
/// </summary>
public class ProjectDashboardDto
{
    /// <summary>
    /// As of date for the dashboard data
    /// </summary>
    public DateTime AsOfDate { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public Guid TenantId { get; set; }

    // Project Portfolio Metrics
    public KpiMetricDto TotalProjects { get; set; } = new();
    public KpiMetricDto ActiveProjects { get; set; } = new();
    public KpiMetricDto OnHoldProjects { get; set; } = new();
    public KpiMetricDto CompletedProjectsThisMonth { get; set; } = new();

    // Budget & Financial Metrics
    public KpiMetricDto TotalBudget { get; set; } = new();
    public KpiMetricDto BudgetUtilized { get; set; } = new();
    public KpiMetricDto BudgetRemaining { get; set; } = new();
    public KpiMetricDto ProjectsOnBudget { get; set; } = new();
    public KpiMetricDto ProjectsOverBudget { get; set; } = new();
    public KpiMetricDto AverageBudgetVariance { get; set; } = new();

    // Time & Resource Metrics
    public KpiMetricDto TotalPlannedHours { get; set; } = new();
    public KpiMetricDto TotalActualHours { get; set; } = new();
    public KpiMetricDto HoursVariance { get; set; } = new();
    public KpiMetricDto ResourceUtilization { get; set; } = new();
    public KpiMetricDto AvailableCapacity { get; set; } = new();

    // Profitability Metrics
    public KpiMetricDto TotalRevenue { get; set; } = new();
    public KpiMetricDto TotalCost { get; set; } = new();
    public KpiMetricDto TotalProfit { get; set; } = new();
    public KpiMetricDto AverageProfitMargin { get; set; } = new();

    // Risk Indicators
    public KpiMetricDto ProjectsAtRisk { get; set; } = new();
    public KpiMetricDto OverdueDeliverables { get; set; } = new();
    public KpiMetricDto UnapprovedTimesheets { get; set; } = new();

    // Project Details
    public List<ProjectPerformanceDto> AllProjects { get; set; } = new();
    public List<ProjectPerformanceDto> AtRiskProjects { get; set; } = new();
    public List<ProjectPerformanceDto> MostProfitableProjects { get; set; } = new();

    // Resource Allocation
    public List<ResourceAllocationDto> ResourceAllocations { get; set; } = new();

    // Chart data
    public List<ChartDataPointDto> BudgetUtilizationTrend { get; set; } = new();
    public List<PieChartDataDto> ProjectsByStatus { get; set; } = new();
    public List<PieChartDataDto> ProjectsByType { get; set; } = new();
}
