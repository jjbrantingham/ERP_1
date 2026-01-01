using ERP.Domain.Common.Interfaces;
using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;

namespace ERP.Domain.BILL.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByClientIdAsync(long clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByInvoiceIdAsync(long invoiceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
}
