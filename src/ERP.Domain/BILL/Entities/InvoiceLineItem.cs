using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;

namespace ERP.Domain.BILL.Entities;

/// <summary>
/// Represents a single line item on an invoice
/// </summary>
public class InvoiceLineItem : Entity
{
    public long InvoiceId { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }

    private InvoiceLineItem()
    {
        Description = null!;
        UnitPrice = null!;
    }

    /// <summary>
    /// Create an invoice line item
    /// </summary>
    public static InvoiceLineItem Create(
        string description,
        decimal quantity,
        Money unitPrice,
        decimal discountPercent = 0m)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice.Amount < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentException("Discount percent must be between 0 and 100", nameof(discountPercent));

        return new InvoiceLineItem
        {
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent
        };
    }

    /// <summary>
    /// Calculate line total (quantity * unit price - discount)
    /// </summary>
    public decimal CalculateLineTotal()
    {
        var lineTotal = Quantity * UnitPrice.Amount;
        var discount = lineTotal * (DiscountPercent / 100);
        var netAmount = lineTotal - discount;

        return Math.Round(netAmount, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calculate discount amount
    /// </summary>
    public decimal CalculateDiscountAmount()
    {
        var lineTotal = Quantity * UnitPrice.Amount;
        var discount = lineTotal * (DiscountPercent / 100);

        return Math.Round(discount, 2, MidpointRounding.AwayFromZero);
    }
}
