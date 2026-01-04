using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Events;
using ERP.Domain.BILL.ValueObjects;

namespace ERP.Domain.BILL.Entities;

/// <summary>
/// Represents an invoice for billing customers
/// </summary>
public class Invoice : AggregateRoot
{
    public InvoiceNumber InvoiceNumber { get; private set; }
    public long ClientId { get; private set; }
    public long? ProjectId { get; private set; }
    public BillingMode BillingMode { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public string? PoNumber { get; private set; }
    public string? Description { get; private set; }

    private readonly List<InvoiceLineItem> _lineItems;
    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    /// <summary>
    /// Tax rate as decimal (e.g., 0.08 for 8%)
    /// </summary>
    public decimal TaxRate { get; private set; }

    /// <summary>
    /// Currency code (ISO 4217)
    /// </summary>
    public string Currency { get; private set; }

    /// <summary>
    /// Amount paid so far
    /// </summary>
    public Money AmountPaid { get; private set; }

    /// <summary>
    /// Total amount of the invoice (subtotal + tax)
    /// Calculated and stored for query performance
    /// </summary>
    public decimal TotalAmount { get; private set; }

    public DateTime? SentDate { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public DateTime? PaidDate { get; private set; }
    public DateTime? VoidedDate { get; private set; }
    public string? VoidReason { get; private set; }

    private Invoice()
    {
        InvoiceNumber = null!;
        Currency = null!;
        AmountPaid = null!;
        _lineItems = new List<InvoiceLineItem>();
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    public static Invoice Create(
        Guid tenantId,
        InvoiceNumber invoiceNumber,
        long clientId,
        DateTime invoiceDate,
        DateTime dueDate,
        BillingMode billingMode,
        string currency = "USD",
        long? projectId = null,
        string? poNumber = null,
        string? description = null,
        decimal taxRate = 0m)
    {
        if (dueDate < invoiceDate)
            throw new ArgumentException("Due date must be on or after invoice date", nameof(dueDate));

        if (taxRate < 0 || taxRate > 1)
            throw new ArgumentException("Tax rate must be between 0 and 1", nameof(taxRate));

        var invoice = new Invoice
        {
            TenantId = tenantId,
            InvoiceNumber = invoiceNumber,
            ClientId = clientId,
            ProjectId = projectId,
            BillingMode = billingMode,
            Status = InvoiceStatus.Draft,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            PoNumber = poNumber,
            Description = description,
            TaxRate = taxRate,
            Currency = currency.ToUpperInvariant(),
            AmountPaid = new Money(0m, currency),
            TotalAmount = 0m, // Initial total is 0 (no line items yet)
            CreatedDate = DateTime.UtcNow
        };

        invoice.AddDomainEvent(new InvoiceCreatedEvent(invoice.Id, tenantId, invoiceNumber.Value));

        return invoice;
    }

    /// <summary>
    /// Add a line item to the invoice
    /// </summary>
    public void AddLineItem(
        string description,
        decimal quantity,
        Money unitPrice,
        decimal discountPercent = 0m)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify non-draft invoices");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Line item description cannot be empty", nameof(description));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentException("Discount percent must be between 0 and 100", nameof(discountPercent));

        if (unitPrice.Currency != Currency)
            throw new InvalidOperationException($"Line item currency ({unitPrice.Currency}) must match invoice currency ({Currency})");

        var lineItem = InvoiceLineItem.Create(description, quantity, unitPrice, discountPercent);
        _lineItems.Add(lineItem);

        // Recalculate total amount
        RecalculateTotalAmount();

        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculate subtotal (before tax)
    /// </summary>
    public Money CalculateSubtotal()
    {
        if (!_lineItems.Any())
            return new Money(0m, Currency);

        var total = _lineItems.Sum(x => x.CalculateLineTotal());
        return new Money(total, Currency);
    }

    /// <summary>
    /// Calculate tax amount
    /// </summary>
    public Money CalculateTax()
    {
        var subtotal = CalculateSubtotal();
        var taxAmount = subtotal.Amount * TaxRate;
        return new Money(taxAmount, Currency);
    }

    /// <summary>
    /// Calculate total amount (subtotal + tax)
    /// </summary>
    public Money CalculateTotal()
    {
        var subtotal = CalculateSubtotal();
        var tax = CalculateTax();
        return subtotal.Add(tax);
    }

    /// <summary>
    /// Calculate amount due (total - amount paid)
    /// </summary>
    public Money CalculateAmountDue()
    {
        var total = CalculateTotal();
        var due = total.Amount - AmountPaid.Amount;
        return new Money(due, Currency);
    }

    /// <summary>
    /// Send invoice to customer
    /// </summary>
    public void Send()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Can only send draft invoices");

        if (_lineItems.Count == 0)
            throw new InvalidOperationException("Cannot send invoice with no line items");

        Status = InvoiceStatus.Sent;
        SentDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new InvoiceSentEvent(Id, TenantId, InvoiceNumber.Value, ClientId));
    }

    /// <summary>
    /// Post invoice to accounting system
    /// Creates journal entry in Financial module
    /// </summary>
    public void Post()
    {
        if (Status != InvoiceStatus.Sent && Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Can only post sent or draft invoices");

        if (_lineItems.Count == 0)
            throw new InvalidOperationException("Cannot post invoice with no line items");

        Status = InvoiceStatus.Posted;
        PostedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;

        var total = CalculateTotal();

        AddDomainEvent(new InvoicePostedEvent(
            Id,
            TenantId,
            InvoiceNumber.Value,
            ClientId,
            total.Amount,
            total.Currency));
    }

    /// <summary>
    /// Apply payment to invoice
    /// </summary>
    public void ApplyPayment(Money payment)
    {
        if (payment.Currency != Currency)
            throw new InvalidOperationException("Payment currency must match invoice currency");

        if (Status != InvoiceStatus.Posted && Status != InvoiceStatus.PartiallyPaid && Status != InvoiceStatus.Overdue)
            throw new InvalidOperationException("Can only apply payments to posted, partially paid, or overdue invoices");

        var total = CalculateTotal();
        var newAmountPaid = AmountPaid.Add(payment);

        if (newAmountPaid.Amount > total.Amount)
            throw new InvalidOperationException($"Payment amount ({payment.Amount:C}) exceeds amount due ({CalculateAmountDue().Amount:C})");

        AmountPaid = newAmountPaid;
        ModifiedDate = DateTime.UtcNow;

        // Update status based on payment
        if (AmountPaid.Amount == total.Amount)
        {
            Status = InvoiceStatus.Paid;
            PaidDate = DateTime.UtcNow;
            AddDomainEvent(new InvoicePaidEvent(Id, TenantId, InvoiceNumber.Value, total.Amount));
        }
        else if (AmountPaid.Amount > 0)
        {
            Status = InvoiceStatus.PartiallyPaid;
        }

        AddDomainEvent(new PaymentAppliedEvent(Id, TenantId, InvoiceNumber.Value, payment.Amount));
    }

    /// <summary>
    /// Mark invoice as overdue
    /// </summary>
    public void MarkOverdue()
    {
        if (Status == InvoiceStatus.Posted || Status == InvoiceStatus.PartiallyPaid)
        {
            if (DateTime.UtcNow > DueDate)
            {
                Status = InvoiceStatus.Overdue;
                ModifiedDate = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Void the invoice
    /// </summary>
    public void Void(string reason)
    {
        if (Status == InvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot void draft invoices. Use Cancel instead.");

        if (Status == InvoiceStatus.Voided || Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException($"Invoice is already {Status}");

        if (AmountPaid.Amount > 0)
            throw new InvalidOperationException("Cannot void invoice with payments. Reverse payments first.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Void reason is required", nameof(reason));

        Status = InvoiceStatus.Voided;
        VoidedDate = DateTime.UtcNow;
        VoidReason = reason;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new InvoiceVoidedEvent(Id, TenantId, InvoiceNumber.Value, reason));
    }

    /// <summary>
    /// Cancel the invoice (draft only)
    /// </summary>
    public void Cancel()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Can only cancel draft invoices");

        Status = InvoiceStatus.Cancelled;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Update invoice details (draft only)
    /// </summary>
    public void Update(
        DateTime? invoiceDate = null,
        DateTime? dueDate = null,
        string? poNumber = null,
        string? description = null,
        decimal? taxRate = null)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify non-draft invoices");

        if (invoiceDate.HasValue)
            InvoiceDate = invoiceDate.Value;

        if (dueDate.HasValue)
        {
            if (dueDate.Value < InvoiceDate)
                throw new ArgumentException("Due date must be on or after invoice date");
            DueDate = dueDate.Value;
        }

        if (poNumber != null)
            PoNumber = poNumber;

        if (description != null)
            Description = description;

        if (taxRate.HasValue)
        {
            if (taxRate.Value < 0 || taxRate.Value > 1)
                throw new ArgumentException("Tax rate must be between 0 and 1");
            TaxRate = taxRate.Value;

            // Recalculate total amount when tax rate changes
            RecalculateTotalAmount();
        }

        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculate and update the total amount
    /// </summary>
    private void RecalculateTotalAmount()
    {
        var total = CalculateTotal();
        TotalAmount = total.Amount;
    }
}
