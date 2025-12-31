using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;
using ERP.Domain.TE.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

public class ExpenseReportRepository : Repository<ExpenseReport>, IExpenseReportRepository
{
    public ExpenseReportRepository(ERPDbContext context) : base(context) { }

    public async Task<ExpenseReport?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseReports
            .Include(e => e.Items)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<ExpenseReport?> GetByReportNumberAsync(string reportNumber, CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseReports
            .FirstOrDefaultAsync(e => e.ReportNumber == reportNumber, cancellationToken);
    }

    public async Task<IEnumerable<ExpenseReport>> GetByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseReports
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(e => e.ReportDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExpenseReport>> GetByStatusAsync(ExpenseStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseReports
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.ReportDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExpenseReport>> GetByEmployeeAndStatusAsync(long employeeId, ExpenseStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseReports
            .Where(e => e.EmployeeId == employeeId && e.Status == status)
            .OrderByDescending(e => e.ReportDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExpenseReport>> GetPendingExpenseReportsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseReports
            .Where(e => e.Status == ExpenseStatus.Submitted)
            .OrderBy(e => e.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExpenseReport>> GetApprovedNotReimbursedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseReports
            .Where(e => e.Status == ExpenseStatus.Approved)
            .OrderBy(e => e.ApprovedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string reportNumber, CancellationToken cancellationToken = default)
    {
        return await _context.ExpenseReports
            .AnyAsync(e => e.ReportNumber == reportNumber, cancellationToken);
    }
}
