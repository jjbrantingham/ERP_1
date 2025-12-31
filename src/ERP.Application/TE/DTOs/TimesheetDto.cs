namespace ERP.Application.TE.DTOs;

public class TimesheetDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public long? ApprovedByUserId { get; set; }
    public string? ApprovalComments { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
