using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.BILL.ValueObjects;
using ERP.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Invoice?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(
        InvoiceNumber invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        var invoiceNumberValue = invoiceNumber.Value;
        return await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.InvoiceNumber.Value == invoiceNumberValue, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByClientIdAsync(
        long clientId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByProjectIdAsync(
        long projectId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(
        InvoiceStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => (i.Status == InvoiceStatus.Posted ||
                        i.Status == InvoiceStatus.PartiallyPaid ||
                        i.Status == InvoiceStatus.Overdue) &&
                       i.DueDate < now)
            .OrderBy(i => i.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.Status == InvoiceStatus.Posted ||
                       i.Status == InvoiceStatus.PartiallyPaid ||
                       i.Status == InvoiceStatus.Overdue)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(InvoiceNumber invoiceNumber, CancellationToken cancellationToken = default)
    {
        var invoiceNumberValue = invoiceNumber.Value;
        return await _context.Invoices
            .AnyAsync(i => i.InvoiceNumber.Value == invoiceNumberValue, cancellationToken);
    }
}
