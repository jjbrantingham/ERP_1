using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;

namespace ERP.UnitTests.Domain.CRM;

/// <summary>
/// Unit tests for Client entity
/// </summary>
public class ClientTests
{
    [Fact]
    public void Create_WithValidData_CreatesClient()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var companyName = "Acme Corp";
        var email = "contact@acme.com";

        // Act
        var client = Client.Create(tenantId, companyName, email);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(tenantId, client.TenantId);
        Assert.Equal(companyName, client.CompanyName);
        Assert.Equal(email, client.Email);
        Assert.Equal(ClientStatus.Active, client.Status);
        Assert.True(client.IsActive);
    }

    [Fact]
    public void Create_WithNullCompanyName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Client.Create(Guid.NewGuid(), null!, "test@example.com"));
        Assert.Contains("Company name is required", exception.Message);
    }

    [Fact]
    public void UpdateInfo_UpdatesCompanyInfo()
    {
        // Arrange
        var client = CreateTestClient();
        var newName = "Acme Corporation";
        var newPhone = "+1-555-0100";

        // Act
        client.UpdateInfo(newName, newPhone, "contact@acme.com");

        // Assert
        Assert.Equal(newName, client.CompanyName);
        Assert.Equal(newPhone, client.Phone);
        Assert.NotNull(client.ModifiedDate);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var client = CreateTestClient();

        // Act
        client.Deactivate();

        // Assert
        Assert.False(client.IsActive);
        Assert.Equal(ClientStatus.Inactive, client.Status);
    }

    private Client CreateTestClient()
    {
        return Client.Create(
            Guid.NewGuid(),
            "Acme Corp",
            "contact@acme.com",
            "+1-555-0100",
            "123 Main St");
    }
}
