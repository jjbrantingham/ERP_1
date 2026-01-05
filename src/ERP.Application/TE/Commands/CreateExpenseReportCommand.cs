using ERP.Application.Common.Interfaces;
namespace ERP.Application.TE.Commands;

public class CreateExpenseReportCommand : ICommand<long>
{
    public long EmployeeId { get; init; }
    public string? Purpose { get; init; }
    public DateTime? ReportDate { get; init; }
    public string? Notes { get; init; }
}
