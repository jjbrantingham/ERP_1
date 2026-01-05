using ERP.Application.Common.Interfaces;
namespace ERP.Application.TE.Commands;

public class CreateTimesheetCommand : ICommand<long>
{
    public long EmployeeId { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public string? Notes { get; init; }
}
