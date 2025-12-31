using ERP.Domain.Common;

namespace ERP.Domain.PM.ValueObjects;

/// <summary>
/// Value object representing a project number.
/// </summary>
public class ProjectNumber : ValueObject
{
    public string Value { get; private set; }

    private ProjectNumber()
    {
        Value = string.Empty;
    }

    public ProjectNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Project number cannot be empty", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("Project number cannot exceed 50 characters", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static ProjectNumber Generate()
    {
        var value = $"PRJ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..26];
        return new ProjectNumber(value);
    }

    public static implicit operator string(ProjectNumber projectNumber) => projectNumber.Value;
}
