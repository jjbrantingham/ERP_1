using ERP.Domain.Common.Events;

namespace ERP.Domain.BILL.Events;

public class PaymentAppliedEvent : DomainEvent
{
    public long InvoiceId { get; }
    public Guid TenantId { get; }
    public string InvoiceNumber { get; }
    public decimal PaymentAmount { get; }

    public PaymentAppliedEvent(long invoiceId, Guid tenantId, string invoiceNumber, decimal paymentAmount)
    {
        InvoiceId = invoiceId;
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        PaymentAmount = paymentAmount;
    }
}
