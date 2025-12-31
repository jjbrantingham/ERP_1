namespace ERP.Application.TE.DTOs;

public class ExpenseReportDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long EmployeeId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public DateTime ReportDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public long? ApprovedByUserId { get; set; }
    public string? ApprovalComments { get; set; }
    public DateTime? ReimbursedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
