namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Project status report
/// </summary>
public class ProjectStatusDto
{
    public long ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualCost { get; set; }
    public decimal BudgetVariance { get; set; }
    public decimal PercentComplete { get; set; }
    public decimal TotalHours { get; set; }
    public decimal BillableHours { get; set; }
    public decimal NonBillableHours { get; set; }
    public int MilestonesCompleted { get; set; }
    public int MilestonesTotal { get; set; }
    public string ProjectManager { get; set; } = string.Empty;
}

/// <summary>
/// Resource utilization report
/// </summary>
public class ResourceUtilizationDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<EmployeeUtilizationDto> Employees { get; set; } = new();
    public decimal AverageUtilization { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal TotalAvailableHours { get; set; }
}

/// <summary>
/// Employee utilization in resource utilization report
/// </summary>
public class EmployeeUtilizationDto
{
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public decimal BillableHours { get; set; }
    public decimal NonBillableHours { get; set; }
    public decimal PtoHours { get; set; }
    public decimal AvailableHours { get; set; }
    public decimal UtilizationRate { get; set; }
    public decimal BillableRate { get; set; }
    public int ProjectCount { get; set; }
}

/// <summary>
/// Budget variance report
/// </summary>
public class BudgetVarianceDto
{
    public long ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public List<BudgetVarianceLineDto> Lines { get; set; } = new();
    public decimal TotalBudget { get; set; }
    public decimal TotalActual { get; set; }
    public decimal TotalVariance { get; set; }
    public decimal VariancePercentage { get; set; }
}

/// <summary>
/// Budget variance line item
/// </summary>
public class BudgetVarianceLineDto
{
    public string Category { get; set; } = string.Empty;
    public string WBSItem { get; set; } = string.Empty;
    public decimal BudgetedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public string Status { get; set; } = string.Empty; // Under, Over, OnTrack
}

/// <summary>
/// Project timeline report (Gantt chart data)
/// </summary>
public class ProjectTimelineDto
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ProjectStart { get; set; }
    public DateTime ProjectEnd { get; set; }
    public List<TimelineTaskDto> Tasks { get; set; } = new();
    public List<TimelineMilestoneDto> Milestones { get; set; } = new();
}

/// <summary>
/// Timeline task for Gantt chart
/// </summary>
public class TimelineTaskDto
{
    public long TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PercentComplete { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public List<long> Dependencies { get; set; } = new();
}

/// <summary>
/// Timeline milestone for Gantt chart
/// </summary>
public class TimelineMilestoneDto
{
    public long MilestoneId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
}
