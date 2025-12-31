namespace ERP.Domain.Common.ValueObjects;

/// <summary>
/// Represents a date range with start and end dates.
/// Immutable value object for handling date periods.
/// </summary>
public class DateRange : ValueObject
{
    /// <summary>
    /// Gets the start date of the range.
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// Gets the end date of the range.
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateRange"/> class.
    /// </summary>
    /// <param name="start">The start date.</param>
    /// <param name="end">The end date.</param>
    /// <exception cref="ArgumentException">Thrown when start date is after end date.</exception>
    public DateRange(DateTime start, DateTime end)
    {
        if (start > end)
            throw new ArgumentException("Start date cannot be after end date.", nameof(start));

        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the number of days in the date range.
    /// </summary>
    public int DurationInDays => (End.Date - Start.Date).Days + 1;

    /// <summary>
    /// Gets the number of months in the date range (approximate).
    /// </summary>
    public int DurationInMonths =>
        ((End.Year - Start.Year) * 12) + End.Month - Start.Month + 1;

    /// <summary>
    /// Checks if a date falls within this range (inclusive).
    /// </summary>
    public bool Contains(DateTime date)
    {
        return date.Date >= Start.Date && date.Date <= End.Date;
    }

    /// <summary>
    /// Checks if this range overlaps with another range.
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        return Start <= other.End && End >= other.Start;
    }

    /// <summary>
    /// Checks if this range completely contains another range.
    /// </summary>
    public bool Contains(DateRange other)
    {
        return Start <= other.Start && End >= other.End;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString()
    {
        return $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd}";
    }
}
