using ERP.Domain.Common.ValueObjects;

namespace ERP.UnitTests.Domain.Common;

/// <summary>
/// Unit tests for Money value object
/// CRITICAL: Tests financial calculation accuracy
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesInstance()
    {
        // Act
        var money = new Money(100.50m, "USD");

        // Assert
        Assert.Equal(100.50m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Money(-100m, "USD"));
        Assert.Contains("cannot be negative", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyCurrency_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(100m, ""));
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsCorrectSum()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(150m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "EUR");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
        Assert.Contains("different currencies", exception.Message);
    }

    [Fact]
    public void Subtract_WithSameCurrency_ReturnsCorrectDifference()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.Equal(70m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Subtract_ResultingInNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(50m, "USD");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => money1.Subtract(money2));
        Assert.Contains("result in negative", exception.Message);
    }

    [Fact]
    public void Multiply_ByPositiveFactor_ReturnsCorrectProduct()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money.Multiply(2.5m);

        // Assert
        Assert.Equal(250m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Multiply_ByZero_ReturnsZero()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money.Multiply(0m);

        // Assert
        Assert.Equal(0m, result.Amount);
    }

    [Fact]
    public void Multiply_ByNegativeFactor_ThrowsArgumentException()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => money.Multiply(-2m));
        Assert.Contains("cannot be negative", exception.Message);
    }

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(100.50m, "USD");

        // Act & Assert
        Assert.Equal(money1, money2);
        Assert.True(money1.Equals(money2));
    }

    [Fact]
    public void Equals_WithDifferentAmount_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act & Assert
        Assert.NotEqual(money1, money2);
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "EUR");

        // Act & Assert
        Assert.NotEqual(money1, money2);
    }

    [Fact]
    public void Round_RoundsToSpecifiedDecimals()
    {
        // Arrange
        var money = new Money(100.12345m, "USD");

        // Act
        var rounded = money.Round(2);

        // Assert
        Assert.Equal(100.12m, rounded.Amount);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var money = new Money(100.50m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal("100.50 USD", result);
    }

    [Theory]
    [InlineData(100.00, 50.00, 150.00)]
    [InlineData(0.01, 0.02, 0.03)]
    [InlineData(999.99, 0.01, 1000.00)]
    public void Add_VariousAmounts_CalculatesCorrectly(decimal amount1, decimal amount2, decimal expected)
    {
        // Arrange
        var money1 = new Money(amount1, "USD");
        var money2 = new Money(amount2, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(expected, result.Amount);
    }
}
