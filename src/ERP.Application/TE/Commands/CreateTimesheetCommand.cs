namespace ERP.Application.TE.Commands;

public class CreateTimesheetCommand
{
    public long EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? Notes { get; set; }
}
