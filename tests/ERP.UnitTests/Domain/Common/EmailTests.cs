using ERP.Domain.Common.ValueObjects;

namespace ERP.UnitTests.Domain.Common;

/// <summary>
/// Unit tests for Email value object
/// </summary>
public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@company.co.uk")]
    [InlineData("first.last+tag@example.com")]
    [InlineData("admin@subdomain.example.com")]
    public void Create_WithValidEmail_CreatesEmail(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        Assert.NotNull(email);
        Assert.Equal(validEmail.ToLower(), email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user name@example.com")]
    public void Create_WithInvalidEmail_ThrowsArgumentException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
    }

    [Fact]
    public void Equals_WithSameEmail_ReturnsTrue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Assert
        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
    }

    [Fact]
    public void Equals_WithDifferentCase_ReturnsTrueAfterNormalization()
    {
        // Arrange
        var email1 = new Email("Test@Example.COM");
        var email2 = new Email("test@example.com");

        // Assert
        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Equals_WithDifferentEmail_ReturnsFalse()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Assert
        Assert.NotEqual(email1, email2);
        Assert.False(email1 == email2);
    }
}
