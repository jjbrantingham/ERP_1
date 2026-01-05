using ERP.Domain.Common;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;

namespace ERP.Domain.TE.Repositories;

/// <summary>
/// Repository interface for ExpenseReport aggregate root.
/// </summary>
public interface IExpenseReportRepository : IRepository<ExpenseReport>
{
    /// <summary>
    /// Get expense report by ID with all items.
    /// </summary>
    Task<ExpenseReport?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expense report by report number.
    /// </summary>
    Task<ExpenseReport?> GetByReportNumberAsync(string reportNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all expense reports for an employee.
    /// </summary>
    Task<IEnumerable<ExpenseReport>> GetByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expense reports by status.
    /// </summary>
    Task<IEnumerable<ExpenseReport>> GetByStatusAsync(ExpenseStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expense reports for an employee by status.
    /// </summary>
    Task<IEnumerable<ExpenseReport>> GetByEmployeeAndStatusAsync(long employeeId, ExpenseStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending expense reports (submitted but not approved).
    /// </summary>
    Task<IEnumerable<ExpenseReport>> GetPendingExpenseReportsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get approved but not reimbursed expense reports.
    /// </summary>
    Task<IEnumerable<ExpenseReport>> GetApprovedNotReimbursedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if report number exists.
    /// </summary>
    Task<bool> ExistsAsync(string reportNumber, CancellationToken cancellationToken = default);
}
