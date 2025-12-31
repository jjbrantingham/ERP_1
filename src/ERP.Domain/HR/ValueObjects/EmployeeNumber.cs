using ERP.Domain.Common;

namespace ERP.Domain.HR.ValueObjects;

/// <summary>
/// Value object representing an employee number.
/// Follows a format like EMP-YYYYMMDD-XXXXXX.
/// </summary>
public class EmployeeNumber : ValueObject
{
    public string Value { get; private set; }

    private EmployeeNumber()
    {
        Value = string.Empty;
    }

    public EmployeeNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Employee number cannot be empty", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("Employee number cannot exceed 50 characters", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static EmployeeNumber Generate()
    {
        var value = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..26];
        return new EmployeeNumber(value);
    }

    public static implicit operator string(EmployeeNumber employeeNumber) => employeeNumber.Value;
}
