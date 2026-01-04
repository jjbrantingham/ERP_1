using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Events;

namespace ERP.Domain.BILL.Entities;

/// <summary>
/// Represents a payment received from a customer
/// </summary>
public class Payment : AggregateRoot
{
    public string PaymentNumber { get; private set; }
    public long ClientId { get; private set; }
    public long? InvoiceId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public DateTime? ClearedDate { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }

    private Payment()
    {
        PaymentNumber = null!;
        Amount = null!;
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    public static Payment Create(
        Guid tenantId,
        string paymentNumber,
        long clientId,
        Money amount,
        PaymentMethod method,
        DateTime paymentDate,
        long? invoiceId = null,
        string? referenceNumber = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(paymentNumber))
            throw new ArgumentException("Payment number is required", nameof(paymentNumber));

        if (amount.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero", nameof(amount));

        var payment = new Payment
        {
            TenantId = tenantId,
            PaymentNumber = paymentNumber,
            ClientId = clientId,
            InvoiceId = invoiceId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            PaymentDate = paymentDate,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            CreatedDate = DateTime.UtcNow
        };

        payment.AddDomainEvent(new PaymentReceivedEvent(
            payment.Id,
            tenantId,
            paymentNumber,
            clientId,
            amount.Amount,
            amount.Currency));

        return payment;
    }

    /// <summary>
    /// Clear the payment (mark as successfully processed)
    /// </summary>
    public void Clear()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot clear {Status} payment");

        Status = PaymentStatus.Cleared;
        ClearedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new PaymentClearedEvent(Id, TenantId, PaymentNumber, Amount.Amount));
    }

    /// <summary>
    /// Mark payment as failed
    /// </summary>
    public void MarkFailed(string reason)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark {Status} payment as failed");

        Status = PaymentStatus.Failed;
        Notes = $"{Notes}\nFailed: {reason}".Trim();
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Reverse the payment (refund)
    /// </summary>
    public void Reverse(string reason)
    {
        if (Status != PaymentStatus.Cleared)
            throw new InvalidOperationException("Can only reverse cleared payments");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reverse reason is required", nameof(reason));

        Status = PaymentStatus.Reversed;
        Notes = $"{Notes}\nReversed: {reason}".Trim();
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new PaymentReversedEvent(Id, TenantId, PaymentNumber, Amount.Amount, reason));
    }
}
