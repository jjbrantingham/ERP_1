using ERP.Domain.Common.ValueObjects;

namespace ERP.UnitTests.Domain.Common;

/// <summary>
/// Unit tests for DateRange value object
/// </summary>
public class DateRangeTests
{
    [Fact]
    public void Create_WithValidDates_CreatesDateRange()
    {
        // Arrange
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        // Act
        var dateRange = new DateRange(start, end);

        // Assert
        Assert.NotNull(dateRange);
        Assert.Equal(start, dateRange.Start);
        Assert.Equal(end, dateRange.End);
    }

    [Fact]
    public void Create_WhenStartDateAfterEndDate_ThrowsArgumentException()
    {
        // Arrange
        var start = new DateTime(2024, 12, 31);
        var end = new DateTime(2024, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DateRange(start, end));
        Assert.Contains("Start date cannot be after end date", exception.Message);
    }

    [Fact]
    public void Create_WhenStartDateEqualsEndDate_CreatesDateRange()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);

        // Act
        var dateRange = new DateRange(date, date);

        // Assert
        Assert.Equal(date, dateRange.Start);
        Assert.Equal(date, dateRange.End);
    }

    [Fact]
    public void DurationInDays_CalculatesCorrectly()
    {
        // Arrange
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var dateRange = new DateRange(start, end);

        // Act
        var duration = dateRange.DurationInDays;

        // Assert
        Assert.Equal(31, duration); // 31 days inclusive
    }

    [Fact]
    public void Contains_DateWithinRange_ReturnsTrue()
    {
        // Arrange
        var dateRange = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));
        var dateToCheck = new DateTime(2024, 1, 15);

        // Act
        var result = dateRange.Contains(dateToCheck);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_DateBeforeRange_ReturnsFalse()
    {
        // Arrange
        var dateRange = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));
        var dateToCheck = new DateTime(2023, 12, 31);

        // Act
        var result = dateRange.Contains(dateToCheck);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Contains_DateAfterRange_ReturnsFalse()
    {
        // Arrange
        var dateRange = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));
        var dateToCheck = new DateTime(2024, 2, 1);

        // Act
        var result = dateRange.Contains(dateToCheck);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithSameDates_ReturnsTrue()
    {
        // Arrange
        var dateRange1 = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));
        var dateRange2 = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        // Assert
        Assert.Equal(dateRange1, dateRange2);
        Assert.True(dateRange1 == dateRange2);
    }

    [Fact]
    public void Equals_WithDifferentDates_ReturnsFalse()
    {
        // Arrange
        var dateRange1 = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));
        var dateRange2 = new DateRange(new DateTime(2024, 2, 1), new DateTime(2024, 2, 28));

        // Assert
        Assert.NotEqual(dateRange1, dateRange2);
        Assert.False(dateRange1 == dateRange2);
    }
}
