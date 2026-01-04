using ERP.Domain.Common;

namespace ERP.Domain.BILL.Events;

public class PaymentReversedEvent : DomainEvent
{
    public long PaymentId { get; }
    public Guid TenantId { get; }
    public string PaymentNumber { get; }
    public decimal Amount { get; }
    public string Reason { get; }

    public PaymentReversedEvent(
        long paymentId,
        Guid tenantId,
        string paymentNumber,
        decimal amount,
        string reason)
    {
        PaymentId = paymentId;
        TenantId = tenantId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        Reason = reason;
    }
}
