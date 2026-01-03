using ERP.Domain.PM.ValueObjects;

namespace ERP.UnitTests.Domain.PM;

/// <summary>
/// Unit tests for ProjectNumber value object
/// </summary>
public class ProjectNumberTests
{
    [Theory]
    [InlineData("PRJ-2024-001")]
    [InlineData("PRJ-001")]
    [InlineData("PROJECT-123")]
    public void Create_WithValidNumber_CreatesProjectNumber(string validNumber)
    {
        // Act
        var projectNumber = new ProjectNumber(validNumber);

        // Assert
        Assert.NotNull(projectNumber);
        Assert.Equal(validNumber, projectNumber.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidNumber_ThrowsArgumentException(string invalidNumber)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ProjectNumber(invalidNumber));
    }

    [Fact]
    public void Equals_WithSameNumber_ReturnsTrue()
    {
        // Arrange
        var number1 = new ProjectNumber("PRJ-2024-001");
        var number2 = new ProjectNumber("PRJ-2024-001");

        // Assert
        Assert.Equal(number1, number2);
        Assert.True(number1 == number2);
    }

    [Fact]
    public void Equals_WithDifferentNumber_ReturnsFalse()
    {
        // Arrange
        var number1 = new ProjectNumber("PRJ-2024-001");
        var number2 = new ProjectNumber("PRJ-2024-002");

        // Assert
        Assert.NotEqual(number1, number2);
        Assert.False(number1 == number2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var number = new ProjectNumber("PRJ-2024-001");

        // Act
        var result = number.ToString();

        // Assert
        Assert.Equal("PRJ-2024-001", result);
    }
}
