using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByPaymentNumberAsync(
        string paymentNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByClientIdAsync(
        long clientId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(
        long invoiceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Pending)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }
}
