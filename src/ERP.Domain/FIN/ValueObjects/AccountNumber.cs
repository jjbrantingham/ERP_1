using ERP.Domain.Common.ValueObjects;

namespace ERP.Domain.FIN.ValueObjects;

/// <summary>
/// Account number value object
/// Format: XXXX-XXX (e.g., 1000-001, 2100-050)
/// </summary>
public class AccountNumber : ValueObject
{
    public string Value { get; }

    public AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account number cannot be empty", nameof(value));

        if (value.Length > 20)
            throw new ArgumentException("Account number cannot exceed 20 characters", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(AccountNumber accountNumber) => accountNumber.Value;
}
