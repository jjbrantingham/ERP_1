namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Employee dashboard with personal metrics and tasks
/// </summary>
public class EmployeeDashboardDto
{
    /// <summary>
    /// As of date for the dashboard data
    /// </summary>
    public DateTime AsOfDate { get; set; }

    /// <summary>
    /// Employee ID
    /// </summary>
    public long EmployeeId { get; set; }

    /// <summary>
    /// Employee name
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    // Time Tracking Metrics
    public KpiMetricDto HoursThisWeek { get; set; } = new();
    public KpiMetricDto HoursThisMonth { get; set; } = new();
    public KpiMetricDto BillableHoursThisMonth { get; set; } = new();
    public KpiMetricDto NonBillableHoursThisMonth { get; set; } = new();
    public KpiMetricDto UtilizationRate { get; set; } = new();
    public KpiMetricDto OvertimeHours { get; set; } = new();

    // Timesheet Status
    public KpiMetricDto PendingTimesheets { get; set; } = new();
    public KpiMetricDto RejectedTimesheets { get; set; } = new();
    public KpiMetricDto OverdueTimesheets { get; set; } = new();

    // Expense Tracking
    public KpiMetricDto ExpensesThisMonth { get; set; } = new();
    public KpiMetricDto PendingExpenses { get; set; } = new();
    public KpiMetricDto ApprovedExpenses { get; set; } = new();
    public KpiMetricDto RejectedExpenses { get; set; } = new();
    public KpiMetricDto ReimbursementDue { get; set; } = new();

    // Project Assignments
    public KpiMetricDto ActiveProjects { get; set; } = new();
    public KpiMetricDto ProjectsCompleted { get; set; } = new();
    public List<ProjectAssignmentDto> CurrentAssignments { get; set; } = new();

    // Time Off & Leave
    public KpiMetricDto VacationDaysAvailable { get; set; } = new();
    public KpiMetricDto SickDaysUsed { get; set; } = new();
    public KpiMetricDto TimeOffRequests { get; set; } = new();

    // Action Items
    public List<ActionItemDto> PendingActions { get; set; } = new();

    // Chart data
    public List<ChartDataPointDto> HoursThisWeekByDay { get; set; } = new();
    public List<PieChartDataDto> HoursByProject { get; set; } = new();
    public List<ChartDataPointDto> UtilizationTrend { get; set; } = new();
}
