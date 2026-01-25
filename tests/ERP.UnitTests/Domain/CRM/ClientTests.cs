using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.Common.ValueObjects;

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
        var clientNumber = "CLI-001";
        var name = "Acme Corp";
        var email = new Email("contact@acme.com");

        // Act
        var client = Client.Create(
            tenantId,
            clientNumber,
            name,
            ClientType.Corporate,
            primaryEmail: email);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(tenantId, client.TenantId);
        Assert.Equal(clientNumber, client.ClientNumber);
        Assert.Equal(name, client.Name);
        Assert.Equal(email.Value, client.PrimaryEmail?.Value);
        Assert.Equal(ClientStatus.Prospect, client.Status);
        Assert.True(client.IsActive);
    }

    [Fact]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Client.Create(
                Guid.NewGuid(),
                "CLI-001",
                null!,
                ClientType.Corporate));
        Assert.Contains("Client name is required", exception.Message);
    }

    [Fact]
    public void Create_WithNullClientNumber_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Client.Create(
                Guid.NewGuid(),
                null!,
                "Acme Corp",
                ClientType.Corporate));
        Assert.Contains("Client number is required", exception.Message);
    }

    [Fact]
    public void UpdateInfo_UpdatesClientInfo()
    {
        // Arrange
        var client = CreateTestClient();
        var newName = "Acme Corporation";
        var newEmail = new Email("newcontact@acme.com");

        // Act
        client.UpdateInfo(
            newName,
            ClientType.Corporate,
            primaryEmail: newEmail);

        // Assert
        Assert.Equal(newName, client.Name);
        Assert.Equal(newEmail.Value, client.PrimaryEmail?.Value);
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

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var client = CreateTestClient();
        client.Deactivate();

        // Act
        client.Activate();

        // Assert
        Assert.True(client.IsActive);
    }

    [Fact]
    public void ChangeStatus_UpdatesStatus()
    {
        // Arrange
        var client = CreateTestClient();

        // Act
        client.ChangeStatus(ClientStatus.Active);

        // Assert
        Assert.Equal(ClientStatus.Active, client.Status);
    }

    private Client CreateTestClient()
    {
        var clientNumber = Client.GenerateClientNumber();
        return Client.Create(
            Guid.NewGuid(),
            clientNumber,
            "Acme Corp",
            ClientType.Corporate,
            primaryEmail: new Email("contact@acme.com"),
            primaryPhone: "+1-555-0100");
    }
}
