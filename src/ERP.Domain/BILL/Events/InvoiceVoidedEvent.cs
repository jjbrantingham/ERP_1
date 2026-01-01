using ERP.Domain.Common.Events;

namespace ERP.Domain.BILL.Events;

public class InvoiceVoidedEvent : DomainEvent
{
    public long InvoiceId { get; }
    public Guid TenantId { get; }
    public string InvoiceNumber { get; }
    public string Reason { get; }

    public InvoiceVoidedEvent(long invoiceId, Guid tenantId, string invoiceNumber, string reason)
    {
        InvoiceId = invoiceId;
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        Reason = reason;
    }
}
