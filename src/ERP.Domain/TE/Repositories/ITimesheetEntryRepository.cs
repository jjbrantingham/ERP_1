using ERP.Domain.Common;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.TE.Entities;

namespace ERP.Domain.TE.Repositories;

/// <summary>
/// Repository interface for TimesheetEntry entity.
/// </summary>
public interface ITimesheetEntryRepository : IRepository<TimesheetEntry>
{
    /// <summary>
    /// Get entry by ID.
    /// </summary>
    Task<TimesheetEntry?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entries for a timesheet.
    /// </summary>
    Task<IEnumerable<TimesheetEntry>> GetByTimesheetIdAsync(long timesheetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entries for a project.
    /// </summary>
    Task<IEnumerable<TimesheetEntry>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get billable entries for a project.
    /// </summary>
    Task<IEnumerable<TimesheetEntry>> GetBillableEntriesAsync(long projectId, CancellationToken cancellationToken = default);
}
