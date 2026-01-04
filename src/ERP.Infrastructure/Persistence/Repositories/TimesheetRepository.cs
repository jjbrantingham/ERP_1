using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;
using ERP.Domain.TE.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

public class TimesheetRepository : Repository<Timesheet>, ITimesheetRepository
{
    public TimesheetRepository(ERPDbContext context) : base(context) { }

    public async Task<Timesheet?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Timesheet>> GetByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Timesheets
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Timesheet>> GetByStatusAsync(TimesheetStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Timesheets
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Timesheet>> GetByEmployeeAndStatusAsync(long employeeId, TimesheetStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Timesheets
            .Where(t => t.EmployeeId == employeeId && t.Status == status)
            .OrderByDescending(t => t.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Timesheet>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Timesheets
            .Where(t => t.PeriodStart >= startDate && t.PeriodEnd <= endDate)
            .OrderByDescending(t => t.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Timesheet>> GetPendingTimesheetsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Timesheets
            .Where(t => t.Status == TimesheetStatus.Submitted)
            .OrderBy(t => t.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForPeriodAsync(long employeeId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
    {
        return await _context.Timesheets
            .AnyAsync(t => t.EmployeeId == employeeId && t.PeriodStart == periodStart && t.PeriodEnd == periodEnd, cancellationToken);
    }
}
