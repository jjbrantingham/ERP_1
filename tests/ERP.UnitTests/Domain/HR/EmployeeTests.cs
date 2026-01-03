using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Enums;

namespace ERP.UnitTests.Domain.HR;

/// <summary>
/// Unit tests for Employee entity
/// </summary>
public class EmployeeTests
{
    [Fact]
    public void Create_WithValidData_CreatesEmployee()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";

        // Act
        var employee = Employee.Create(tenantId, firstName, lastName, email);

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(tenantId, employee.TenantId);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(email, employee.Email);
        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.True(employee.IsActive);
    }

    [Fact]
    public void Create_WithNullFirstName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Employee.Create(Guid.NewGuid(), null!, "Doe", "test@example.com"));
        Assert.Contains("First name is required", exception.Message);
    }

    [Fact]
    public void Create_WithNullLastName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Employee.Create(Guid.NewGuid(), "John", null!, "test@example.com"));
        Assert.Contains("Last name is required", exception.Message);
    }

    [Fact]
    public void UpdateContactInfo_UpdatesEmailAndPhone()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var newEmail = "newemail@example.com";
        var newPhone = "+1-555-0123";

        // Act
        employee.UpdateContactInfo(newEmail, newPhone);

        // Assert
        Assert.Equal(newEmail, employee.Email);
        Assert.Equal(newPhone, employee.Phone);
        Assert.NotNull(employee.ModifiedDate);
    }

    [Fact]
    public void Deactivate_SetsStatusAndIsActive()
    {
        // Arrange
        var employee = CreateTestEmployee();

        // Act
        employee.Deactivate();

        // Assert
        Assert.Equal(EmployeeStatus.Terminated, employee.Status);
        Assert.False(employee.IsActive);
    }

    [Fact]
    public void Reactivate_SetsStatusAndIsActive()
    {
        // Arrange
        var employee = CreateTestEmployee();
        employee.Deactivate();

        // Act
        employee.Reactivate();

        // Assert
        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.True(employee.IsActive);
    }

    private Employee CreateTestEmployee()
    {
        return Employee.Create(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john.doe@example.com",
            "Software Engineer",
            DateTime.UtcNow);
    }
}
