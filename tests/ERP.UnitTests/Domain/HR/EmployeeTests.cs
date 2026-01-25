using ERP.Domain.Common.ValueObjects;
using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Enums;
using ERP.Domain.HR.ValueObjects;

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
        var employeeNumber = new EmployeeNumber("EMP-001");
        var resourceTypeId = 1L;
        var firstName = "John";
        var lastName = "Doe";
        var email = new Email("john.doe@example.com");
        var employmentType = EmploymentType.FullTime;
        var hireDate = DateTime.UtcNow;

        // Act
        var employee = Employee.Create(
            tenantId,
            employeeNumber,
            resourceTypeId,
            firstName,
            lastName,
            email,
            employmentType,
            hireDate);

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(tenantId, employee.TenantId);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(email.Value, employee.Email.Value);
        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.True(employee.IsAvailableForProjects);
    }

    [Fact]
    public void Create_WithNullFirstName_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var employeeNumber = new EmployeeNumber("EMP-001");
        var resourceTypeId = 1L;
        var email = new Email("test@example.com");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Employee.Create(
                tenantId,
                employeeNumber,
                resourceTypeId,
                null!,
                "Doe",
                email,
                EmploymentType.FullTime,
                DateTime.UtcNow));
        Assert.Contains("First name", exception.Message);
    }

    [Fact]
    public void Create_WithNullLastName_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var employeeNumber = new EmployeeNumber("EMP-001");
        var resourceTypeId = 1L;
        var email = new Email("test@example.com");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Employee.Create(
                tenantId,
                employeeNumber,
                resourceTypeId,
                "John",
                null!,
                email,
                EmploymentType.FullTime,
                DateTime.UtcNow));
        Assert.Contains("Last name", exception.Message);
    }

    [Fact]
    public void UpdatePersonalInfo_UpdatesNames()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newEmail = new Email("jane.smith@example.com");

        // Act
        employee.UpdatePersonalInfo(newFirstName, newLastName, newEmail);

        // Assert
        Assert.Equal(newFirstName, employee.FirstName);
        Assert.Equal(newLastName, employee.LastName);
        Assert.Equal(newEmail.Value, employee.Email.Value);
    }

    [Fact]
    public void ChangeStatus_ToTerminated_SetsStatusAndTerminationDate()
    {
        // Arrange
        var employee = CreateTestEmployee();

        // Act
        employee.ChangeStatus(EmployeeStatus.Terminated, "Contract ended");

        // Assert
        Assert.Equal(EmployeeStatus.Terminated, employee.Status);
        Assert.NotNull(employee.TerminationDate);
        Assert.False(employee.IsAvailableForProjects);
    }

    [Fact]
    public void ChangeStatus_ToActive_SetsStatusToActive()
    {
        // Arrange
        var employee = CreateTestEmployee();
        employee.ChangeStatus(EmployeeStatus.OnLeave);

        // Act
        employee.ChangeStatus(EmployeeStatus.Active);

        // Assert
        Assert.Equal(EmployeeStatus.Active, employee.Status);
    }

    [Fact]
    public void FullName_ReturnsCorrectCombination()
    {
        // Arrange
        var employee = CreateTestEmployee();

        // Act
        var fullName = employee.FullName;

        // Assert
        Assert.Equal("John Doe", fullName);
    }

    private Employee CreateTestEmployee()
    {
        return Employee.Create(
            Guid.NewGuid(),
            new EmployeeNumber("EMP-TEST-001"),
            1L,
            "John",
            "Doe",
            new Email("john.doe@example.com"),
            EmploymentType.FullTime,
            DateTime.UtcNow,
            jobTitle: "Software Engineer");
    }
}
