using ERP.Application.Common.Interfaces;
namespace ERP.Application.TE.DTOs;

namespace ERP.Application.TE.Queries;

public class GetTimesheetByIdQuery : IQuery<TimesheetDto>
{
    public long TimesheetId { get; set; }
}
