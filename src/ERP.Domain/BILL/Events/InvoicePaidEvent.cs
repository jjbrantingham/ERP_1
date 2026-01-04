using ERP.Domain.Common;

namespace ERP.Domain.BILL.Events;

public class InvoicePaidEvent : DomainEvent
{
    public long InvoiceId { get; }
    public Guid TenantId { get; }
    public string InvoiceNumber { get; }
    public decimal TotalAmount { get; }

    public InvoicePaidEvent(long invoiceId, Guid tenantId, string invoiceNumber, decimal totalAmount)
    {
        InvoiceId = invoiceId;
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        TotalAmount = totalAmount;
    }
}
