namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Resource allocation summary for dashboards
/// </summary>
public class ResourceAllocationDto
{
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal AllocatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal AllocationPercent { get; set; }
    public decimal UtilizationPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
