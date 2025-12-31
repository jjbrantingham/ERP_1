namespace ERP.Application.TE.Commands;

public class CreateExpenseReportCommand
{
    public long EmployeeId { get; set; }
    public string? Purpose { get; set; }
    public DateTime? ReportDate { get; set; }
    public string? Notes { get; set; }
}
