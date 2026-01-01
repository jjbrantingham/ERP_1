using ERP.Domain.Common.Events;

namespace ERP.Domain.BILL.Events;

public class PaymentClearedEvent : DomainEvent
{
    public long PaymentId { get; }
    public Guid TenantId { get; }
    public string PaymentNumber { get; }
    public decimal Amount { get; }

    public PaymentClearedEvent(long paymentId, Guid tenantId, string paymentNumber, decimal amount)
    {
        PaymentId = paymentId;
        TenantId = tenantId;
        PaymentNumber = paymentNumber;
        Amount = amount;
    }
}
