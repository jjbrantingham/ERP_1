using ERP.Application.RPT.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.RPT.Queries;

public class GetTimesheetSummaryQueryHandler : IRequestHandler<GetTimesheetSummaryQuery, TimesheetSummaryDto>
{
    private readonly IDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetTimesheetSummaryQueryHandler(IDbContext context,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _context = context;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<TimesheetSummaryDto> Handle(GetTimesheetSummaryQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Get timesheets for the period
        var query = _context.Timesheets
            .Include(t => t.Entries)
            .Where(t => t.PeriodStart >= request.StartDate && t.PeriodEnd <= request.EndDate);

        if (request.EmployeeId.HasValue)
            query = query.Where(t => t.EmployeeId == request.EmployeeId.Value);

        var timesheets = await query.ToListAsync(cancellationToken);

        // Group by employee and project
        var grouped = timesheets
            .SelectMany(t => t.Entries.Select(e => new
            {
                EmployeeId = t.EmployeeId,
                t.Status,
                e.ProjectId,
                e.Hours,
                e.IsBillable
            }))
            .GroupBy(x => new { x.EmployeeId, x.ProjectId })
            .Select(g => new TimesheetSummaryLineDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = $"Employee {g.Key.EmployeeId}", // Would need employee lookup
                ProjectId = g.Key.ProjectId,
                ProjectName = g.Key.ProjectId.HasValue ? $"Project {g.Key.ProjectId}" : null,
                TotalHours = g.Sum(x => x.Hours),
                BillableHours = g.Where(x => x.IsBillable).Sum(x => x.Hours),
                NonBillableHours = g.Where(x => !x.IsBillable).Sum(x => x.Hours),
                BillableRate = 150m, // Would need rate lookup
                BillableAmount = g.Where(x => x.IsBillable).Sum(x => x.Hours) * 150m,
                TimesheetCount = timesheets.Count(t => t.EmployeeId == g.Key.EmployeeId)
            })
            .ToList();

        var result = new TimesheetSummaryDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Entries = grouped,
            Totals = new TimesheetSummaryTotalsDto
            {
                TotalHours = grouped.Sum(e => e.TotalHours),
                TotalBillableHours = grouped.Sum(e => e.BillableHours),
                TotalNonBillableHours = grouped.Sum(e => e.NonBillableHours),
                TotalBillableAmount = grouped.Sum(e => e.BillableAmount)
            }
        };

        result.Totals.BillablePercentage = result.Totals.TotalHours > 0
            ? result.Totals.TotalBillableHours / result.Totals.TotalHours * 100
            : 0m;

        return result;
    }
}
