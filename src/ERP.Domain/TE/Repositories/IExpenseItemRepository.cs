using ERP.Domain.Common.Interfaces;
using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;

namespace ERP.Domain.TE.Repositories;

/// <summary>
/// Repository interface for ExpenseItem entity.
/// </summary>
public interface IExpenseItemRepository : IRepository<ExpenseItem>
{
    /// <summary>
    /// Get item by ID.
    /// </summary>
    Task<ExpenseItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all items for an expense report.
    /// </summary>
    Task<IEnumerable<ExpenseItem>> GetByExpenseReportIdAsync(long expenseReportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all items for a project.
    /// </summary>
    Task<IEnumerable<ExpenseItem>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all items by category.
    /// </summary>
    Task<IEnumerable<ExpenseItem>> GetByCategoryAsync(ExpenseCategory category, CancellationToken cancellationToken = default);
}
