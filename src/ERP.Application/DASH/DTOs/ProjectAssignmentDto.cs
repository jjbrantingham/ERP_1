using ERP.Domain.PM.Enums;

namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Project assignment for employee dashboard
/// </summary>
public class ProjectAssignmentDto
{
    public long ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public decimal AllocatedHours { get; set; }
    public decimal HoursUsed { get; set; }
    public decimal HoursRemaining { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
