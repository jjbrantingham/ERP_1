using ERP.Domain.Common;

namespace ERP.Domain.BILL.Events;

public class InvoiceSentEvent : DomainEvent
{
    public long InvoiceId { get; }
    public Guid TenantId { get; }
    public string InvoiceNumber { get; }
    public long ClientId { get; }

    public InvoiceSentEvent(long invoiceId, Guid tenantId, string invoiceNumber, long clientId)
    {
        InvoiceId = invoiceId;
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        ClientId = clientId;
    }
}
