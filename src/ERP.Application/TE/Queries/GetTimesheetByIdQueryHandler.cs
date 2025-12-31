using ERP.Application.Common.Interfaces;
using ERP.Application.TE.DTOs;
using ERP.Domain.TE.Repositories;

namespace ERP.Application.TE.Queries;

public class GetTimesheetByIdQueryHandler : IQueryHandler<GetTimesheetByIdQuery, TimesheetDto>
{
    private readonly ITimesheetRepository _timesheetRepository;

    public GetTimesheetByIdQueryHandler(ITimesheetRepository timesheetRepository)
    {
        _timesheetRepository = timesheetRepository;
    }

    public async Task<TimesheetDto> Handle(GetTimesheetByIdQuery query, CancellationToken cancellationToken = default)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(query.TimesheetId, cancellationToken);
        if (timesheet == null)
            throw new KeyNotFoundException($"Timesheet with ID {query.TimesheetId} not found.");

        return new TimesheetDto
        {
            Id = timesheet.Id,
            TenantId = timesheet.TenantId,
            EmployeeId = timesheet.EmployeeId,
            PeriodStart = timesheet.PeriodStart,
            PeriodEnd = timesheet.PeriodEnd,
            Status = timesheet.Status.ToString(),
            TotalHours = timesheet.TotalHours,
            SubmittedDate = timesheet.SubmittedDate,
            ApprovedDate = timesheet.ApprovedDate,
            ApprovedByUserId = timesheet.ApprovedByUserId,
            ApprovalComments = timesheet.ApprovalComments,
            Notes = timesheet.Notes,
            CreatedDate = timesheet.CreatedDate,
            ModifiedDate = timesheet.ModifiedDate
        };
    }
}
