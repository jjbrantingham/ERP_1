namespace ERP.Application.TE.Commands;

public class CreateExpenseReportCommand
{
    public long EmployeeId { get; init; }
    public string? Purpose { get; init; }
    public DateTime? ReportDate { get; init; }
    public string? Notes { get; init; }
}
