using ERP.Domain.Common;

namespace ERP.Domain.VM.ValueObjects;

/// <summary>
/// Value object representing a unique vendor number.
/// Format: VEN-YYYYMMDD-XXXXXX (e.g., VEN-20250101-A1B2C3)
/// </summary>
public class VendorNumber : ValueObject
{
    public string Value { get; private set; }

    private VendorNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new VendorNumber from a string value.
    /// </summary>
    public static VendorNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Vendor number cannot be empty", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("Vendor number cannot exceed 50 characters", nameof(value));

        return new VendorNumber(value);
    }

    /// <summary>
    /// Generates a new unique vendor number.
    /// </summary>
    public static VendorNumber Generate()
    {
        var value = $"VEN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..26];
        return new VendorNumber(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    // Implicit conversion to string
    public static implicit operator string(VendorNumber vendorNumber) => vendorNumber.Value;

    // Explicit conversion from string
    public static explicit operator VendorNumber(string value) => Create(value);
}
