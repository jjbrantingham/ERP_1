using ERP.Application.RPT.DTOs;
using MediatR;

namespace ERP.Application.RPT.Queries;

public class GetTimesheetSummaryQuery : IRequest<TimesheetSummaryDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long? EmployeeId { get; set; }
    public long? ProjectId { get; set; }
}
