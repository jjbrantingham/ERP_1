using ERP.Domain.Common.Events;

namespace ERP.Domain.BILL.Events;

public class InvoiceCreatedEvent : DomainEvent
{
    public long InvoiceId { get; }
    public Guid TenantId { get; }
    public string InvoiceNumber { get; }

    public InvoiceCreatedEvent(long invoiceId, Guid tenantId, string invoiceNumber)
    {
        InvoiceId = invoiceId;
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
    }
}
