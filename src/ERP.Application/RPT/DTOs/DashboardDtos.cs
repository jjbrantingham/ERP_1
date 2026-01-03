namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Executive dashboard data
/// </summary>
public class ExecutiveDashboardDto
{
    public DateTime AsOfDate { get; set; }
    public FinancialKPIsDto FinancialKPIs { get; set; } = new();
    public ProjectKPIsDto ProjectKPIs { get; set; } = new();
    public OperationalKPIsDto OperationalKPIs { get; set; } = new();
    public List<TopClientDto> TopClients { get; set; } = new();
    public List<TopProjectDto> TopProjects { get; set; } = new();
    public List<TrendDataDto> RevenueTrend { get; set; } = new();
    public List<TrendDataDto> ProfitTrend { get; set; } = new();
}

/// <summary>
/// Financial KPIs
/// </summary>
public class FinancialKPIsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }
    public decimal CashBalance { get; set; }
    public decimal AccountsReceivable { get; set; }
    public decimal AccountsPayable { get; set; }
    public decimal OutstandingInvoices { get; set; }
    public decimal AverageDaysToCollect { get; set; }
}

/// <summary>
/// Project KPIs
/// </summary>
public class ProjectKPIsDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int OnHoldProjects { get; set; }
    public int CompletedProjects { get; set; }
    public decimal AverageProjectProfitMargin { get; set; }
    public decimal TotalProjectRevenue { get; set; }
    public decimal TotalProjectCost { get; set; }
    public int ProjectsOnBudget { get; set; }
    public int ProjectsOverBudget { get; set; }
    public int ProjectsUnderBudget { get; set; }
}

/// <summary>
/// Operational KPIs
/// </summary>
public class OperationalKPIsDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public decimal AverageUtilizationRate { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal TotalNonBillableHours { get; set; }
    public decimal AverageBillableRate { get; set; }
    public int PendingTimesheets { get; set; }
    public int PendingExpenseReports { get; set; }
    public int PendingApprovals { get; set; }
}

/// <summary>
/// Top client summary
/// </summary>
public class TopClientDto
{
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int ProjectCount { get; set; }
    public decimal OutstandingBalance { get; set; }
}

/// <summary>
/// Top project summary
/// </summary>
public class TopProjectDto
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal ProfitMargin { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Trend data point
/// </summary>
public class TrendDataDto
{
    public DateTime Date { get; set; }
    public string Period { get; set; } = string.Empty; // "Jan 2024", "Q1 2024", etc.
    public decimal Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Project Manager dashboard
/// </summary>
public class ProjectManagerDashboardDto
{
    public DateTime AsOfDate { get; set; }
    public List<ProjectStatusDto> MyProjects { get; set; } = new();
    public List<TaskSummaryDto> UpcomingTasks { get; set; } = new();
    public List<MilestoneSummaryDto> UpcomingMilestones { get; set; } = new();
    public ResourceUtilizationDto TeamUtilization { get; set; } = new();
    public decimal TeamBudgetUtilization { get; set; }
    public int PendingApprovals { get; set; }
}

/// <summary>
/// Task summary
/// </summary>
public class TaskSummaryDto
{
    public long TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public decimal PercentComplete { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Milestone summary
/// </summary>
public class MilestoneSummaryDto
{
    public long MilestoneId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public int DaysUntilDue { get; set; }
}

/// <summary>
/// Finance dashboard
/// </summary>
public class FinanceDashboardDto
{
    public DateTime AsOfDate { get; set; }
    public FinancialKPIsDto KPIs { get; set; } = new();
    public ARAgingDto ARAgingSummary { get; set; } = new();
    public List<InvoiceAgingLineDto> RecentInvoices { get; set; } = new();
    public List<PaymentSummaryDto> RecentPayments { get; set; } = new();
    public List<TrendDataDto> CashFlowTrend { get; set; } = new();
    public decimal ProjectedRevenue { get; set; }
    public decimal ProjectedExpenses { get; set; }
}

/// <summary>
/// Payment summary
/// </summary>
public class PaymentSummaryDto
{
    public long PaymentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
}

/// <summary>
/// Employee dashboard
/// </summary>
public class EmployeeDashboardDto
{
    public DateTime AsOfDate { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<TimesheetStatusDto> RecentTimesheets { get; set; } = new();
    public List<ExpenseReportStatusDto> RecentExpenseReports { get; set; } = new();
    public List<ProjectAssignmentDto> CurrentAssignments { get; set; } = new();
    public decimal CurrentPeriodHours { get; set; }
    public decimal CurrentPeriodBillableHours { get; set; }
    public decimal UtilizationRate { get; set; }
    public int PendingApprovals { get; set; }
}

/// <summary>
/// Timesheet status summary
/// </summary>
public class TimesheetStatusDto
{
    public long TimesheetId { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public decimal TotalHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
}

/// <summary>
/// Expense report status summary
/// </summary>
public class ExpenseReportStatusDto
{
    public long ExpenseReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

/// <summary>
/// Project assignment summary
/// </summary>
public class ProjectAssignmentDto
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal AllocatedHoursPerWeek { get; set; }
    public decimal ActualHoursThisWeek { get; set; }
}
