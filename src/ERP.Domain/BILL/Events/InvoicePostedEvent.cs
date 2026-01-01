using ERP.Domain.Common.Events;

namespace ERP.Domain.BILL.Events;

public class InvoicePostedEvent : DomainEvent
{
    public long InvoiceId { get; }
    public Guid TenantId { get; }
    public string InvoiceNumber { get; }
    public long ClientId { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }

    public InvoicePostedEvent(
        long invoiceId,
        Guid tenantId,
        string invoiceNumber,
        long clientId,
        decimal totalAmount,
        string currency)
    {
        InvoiceId = invoiceId;
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        ClientId = clientId;
        TotalAmount = totalAmount;
        Currency = currency;
    }
}
