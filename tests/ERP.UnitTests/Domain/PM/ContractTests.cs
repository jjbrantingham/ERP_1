using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;

namespace ERP.UnitTests.Domain.PM;

/// <summary>
/// Unit tests for Contract entity
/// </summary>
public class ContractTests
{
    [Fact]
    public void Create_WithValidData_CreatesContract()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var projectId = 1L;
        var contractNumber = "CON-2024-001";
        var contractType = ContractType.FixedPrice;
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddMonths(6);

        // Act
        var contract = Contract.Create(
            tenantId,
            projectId,
            contractNumber,
            contractType,
            startDate,
            endDate);

        // Assert
        Assert.NotNull(contract);
        Assert.Equal(tenantId, contract.TenantId);
        Assert.Equal(projectId, contract.ProjectId);
        Assert.Equal(contractNumber, contract.ContractNumber);
        Assert.Equal(contractType, contract.ContractType);
        Assert.Equal(startDate, contract.StartDate);
        Assert.Equal(endDate, contract.EndDate);
        Assert.True(contract.IsActive);
    }

    [Fact]
    public void Create_WithOptionalFields_SetsFieldsCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var title = "Website Development Contract";
        var description = "Full website redesign";
        var contractValue = new Money(50000m, "USD");
        var signedDate = DateTime.UtcNow.AddDays(-5);
        var terms = "Payment terms: Net 30";

        // Act
        var contract = Contract.Create(
            tenantId,
            1L,
            "CON-2024-001",
            ContractType.TimeAndMaterials,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(6),
            title,
            description,
            contractValue,
            signedDate,
            terms);

        // Assert
        Assert.Equal(title, contract.Title);
        Assert.Equal(description, contract.Description);
        Assert.Equal(contractValue, contract.ContractValue);
        Assert.Equal(signedDate, contract.SignedDate);
        Assert.Equal(terms, contract.Terms);
    }

    [Fact]
    public void Create_TrimsWhitespace_FromStringProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var contract = Contract.Create(
            tenantId,
            1L,
            "  CON-2024-001  ",
            ContractType.FixedPrice,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(6),
            "  Title  ",
            "  Description  ",
            null,
            null,
            "  Terms  ");

        // Assert
        Assert.Equal("CON-2024-001", contract.ContractNumber);
        Assert.Equal("Title", contract.Title);
        Assert.Equal("Description", contract.Description);
        Assert.Equal("Terms", contract.Terms);
    }

    [Fact]
    public void Update_WithValidData_UpdatesContract()
    {
        // Arrange
        var contract = CreateTestContract();
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newContractValue = new Money(75000m, "USD");
        var newStartDate = DateTime.UtcNow.AddDays(10);
        var newEndDate = DateTime.UtcNow.AddMonths(12);
        var newSignedDate = DateTime.UtcNow;
        var newTerms = "New payment terms";

        // Act
        contract.Update(
            newTitle,
            newDescription,
            newContractValue,
            newStartDate,
            newEndDate,
            newSignedDate,
            newTerms);

        // Assert
        Assert.Equal(newTitle, contract.Title);
        Assert.Equal(newDescription, contract.Description);
        Assert.Equal(newContractValue, contract.ContractValue);
        Assert.Equal(newStartDate, contract.StartDate);
        Assert.Equal(newEndDate, contract.EndDate);
        Assert.Equal(newSignedDate, contract.SignedDate);
        Assert.Equal(newTerms, contract.Terms);
        Assert.NotNull(contract.ModifiedDate);
    }

    [Fact]
    public void Update_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var contract = CreateTestContract();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            contract.Update(null, null, null, startDate, endDate, null, null));
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var contract = CreateTestContract();
        contract.Deactivate();
        Assert.False(contract.IsActive);

        // Act
        contract.Activate();

        // Assert
        Assert.True(contract.IsActive);
        Assert.NotNull(contract.ModifiedDate);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var contract = CreateTestContract();
        Assert.True(contract.IsActive);

        // Act
        contract.Deactivate();

        // Assert
        Assert.False(contract.IsActive);
        Assert.NotNull(contract.ModifiedDate);
    }

    [Theory]
    [InlineData(ContractType.FixedPrice)]
    [InlineData(ContractType.TimeAndMaterials)]
    [InlineData(ContractType.CostPlus)]
    [InlineData(ContractType.NotToExceed)]
    [InlineData(ContractType.Retainer)]
    public void Create_WithDifferentContractTypes_CreatesSuccessfully(ContractType contractType)
    {
        // Arrange & Act
        var contract = Contract.Create(
            Guid.NewGuid(),
            1L,
            "CON-2024-001",
            contractType,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(6));

        // Assert
        Assert.Equal(contractType, contract.ContractType);
    }

    private Contract CreateTestContract()
    {
        return Contract.Create(
            Guid.NewGuid(),
            projectId: 1L,
            contractNumber: "CON-2024-001",
            contractType: ContractType.FixedPrice,
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(6),
            title: "Test Contract",
            description: "Test Description",
            contractValue: new Money(50000m, "USD"));
    }
}
