namespace ERP.Application.PM.DTOs;

/// <summary>
/// Data transfer object for ResourceAllocation.
/// </summary>
public class ResourceAllocationDto
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public long EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal AllocatedHoursPerWeek { get; set; }
    public string? Role { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
