namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Executive dashboard with high-level company metrics
/// </summary>
public class ExecutiveDashboardDto
{
    /// <summary>
    /// As of date for the dashboard data
    /// </summary>
    public DateTime AsOfDate { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public Guid TenantId { get; set; }

    // Financial Overview
    public KpiMetricDto MonthlyRevenue { get; set; } = new();
    public KpiMetricDto MonthlyProfit { get; set; } = new();
    public KpiMetricDto ProfitMargin { get; set; } = new();
    public KpiMetricDto CashBalance { get; set; } = new();
    public KpiMetricDto AccountsReceivable { get; set; } = new();
    public KpiMetricDto AccountsPayable { get; set; } = new();

    // Project Overview
    public KpiMetricDto ActiveProjects { get; set; } = new();
    public KpiMetricDto ProjectsOnBudget { get; set; } = new();
    public KpiMetricDto ProjectsOverBudget { get; set; } = new();
    public KpiMetricDto AverageProjectMargin { get; set; } = new();

    // Employee Overview
    public KpiMetricDto TotalEmployees { get; set; } = new();
    public KpiMetricDto EmployeeUtilization { get; set; } = new();
    public KpiMetricDto BillableHours { get; set; } = new();
    public KpiMetricDto NonBillableHours { get; set; } = new();

    // Client Overview
    public KpiMetricDto ActiveClients { get; set; } = new();
    public KpiMetricDto NewClientsThisMonth { get; set; } = new();
    public KpiMetricDto ClientRetentionRate { get; set; } = new();

    // Top performing projects (by revenue/profit)
    public List<ProjectPerformanceDto> TopProjects { get; set; } = new();

    // Recent activity highlights
    public List<ActivityHighlightDto> RecentActivities { get; set; } = new();
}
