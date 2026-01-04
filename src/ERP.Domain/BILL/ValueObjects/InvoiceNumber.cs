using ERP.Domain.Common;

namespace ERP.Domain.BILL.ValueObjects;

/// <summary>
/// Invoice number value object
/// Auto-generated format: INV-YYYYMMDD-XXXXXX
/// </summary>
public class InvoiceNumber : ValueObject
{
    public string Value { get; }

    public InvoiceNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invoice number cannot be empty", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("Invoice number cannot exceed 50 characters", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Generate a new invoice number
    /// Format: INV-YYYYMMDD-XXXXXX
    /// </summary>
    public static InvoiceNumber Generate()
    {
        var value = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..27];
        return new InvoiceNumber(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(InvoiceNumber number) => number.Value;
}
