using ERP.Domain.Common;

namespace ERP.Domain.FIN.ValueObjects;

/// <summary>
/// Journal entry number value object
/// Auto-generated format: JE-YYYYMMDD-XXXXXX
/// </summary>
public class JournalEntryNumber : ValueObject
{
    public string Value { get; }

    public JournalEntryNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Journal entry number cannot be empty", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("Journal entry number cannot exceed 50 characters", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Generate a new journal entry number
    /// Format: JE-YYYYMMDD-XXXXXX
    /// </summary>
    public static JournalEntryNumber Generate()
    {
        var value = $"JE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..26];
        return new JournalEntryNumber(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(JournalEntryNumber number) => number.Value;
}
