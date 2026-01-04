using ERP.Domain.Common;

namespace ERP.Domain.BILL.Events;

public class PaymentReceivedEvent : DomainEvent
{
    public long PaymentId { get; }
    public Guid TenantId { get; }
    public string PaymentNumber { get; }
    public long ClientId { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public PaymentReceivedEvent(
        long paymentId,
        Guid tenantId,
        string paymentNumber,
        long clientId,
        decimal amount,
        string currency)
    {
        PaymentId = paymentId;
        TenantId = tenantId;
        PaymentNumber = paymentNumber;
        ClientId = clientId;
        Amount = amount;
        Currency = currency;
    }
}
