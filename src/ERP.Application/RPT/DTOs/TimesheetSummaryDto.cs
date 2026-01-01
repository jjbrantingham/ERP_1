namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Timesheet summary report
/// </summary>
public class TimesheetSummaryDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TimesheetSummaryLineDto> Entries { get; set; } = new();
    public TimesheetSummaryTotalsDto Totals { get; set; } = new();
}

public class TimesheetSummaryLineDto
{
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public long? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public decimal TotalHours { get; set; }
    public decimal BillableHours { get; set; }
    public decimal NonBillableHours { get; set; }
    public decimal BillableRate { get; set; }
    public decimal BillableAmount { get; set; }
    public int TimesheetCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
}

public class TimesheetSummaryTotalsDto
{
    public decimal TotalHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal TotalNonBillableHours { get; set; }
    public decimal TotalBillableAmount { get; set; }
    public decimal BillablePercentage { get; set; }
}
