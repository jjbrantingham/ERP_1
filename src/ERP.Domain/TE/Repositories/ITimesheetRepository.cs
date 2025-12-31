using ERP.Domain.Common.Interfaces;
using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;

namespace ERP.Domain.TE.Repositories;

/// <summary>
/// Repository interface for Timesheet aggregate root.
/// </summary>
public interface ITimesheetRepository : IRepository<Timesheet>
{
    /// <summary>
    /// Get timesheet by ID with all entries.
    /// </summary>
    Task<Timesheet?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all timesheets for an employee.
    /// </summary>
    Task<IEnumerable<Timesheet>> GetByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get timesheets by status.
    /// </summary>
    Task<IEnumerable<Timesheet>> GetByStatusAsync(TimesheetStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get timesheets for an employee by status.
    /// </summary>
    Task<IEnumerable<Timesheet>> GetByEmployeeAndStatusAsync(long employeeId, TimesheetStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get timesheets for a date range.
    /// </summary>
    Task<IEnumerable<Timesheet>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending timesheets (submitted but not approved).
    /// </summary>
    Task<IEnumerable<Timesheet>> GetPendingTimesheetsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if timesheet exists for employee and period.
    /// </summary>
    Task<bool> ExistsForPeriodAsync(long employeeId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default);
}
