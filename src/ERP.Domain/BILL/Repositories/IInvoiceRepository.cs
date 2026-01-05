using ERP.Domain.Common;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.ValueObjects;

namespace ERP.Domain.BILL.Repositories;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(InvoiceNumber invoiceNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByClientIdAsync(long clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetUnpaidInvoicesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(InvoiceNumber invoiceNumber, CancellationToken cancellationToken = default);
}
