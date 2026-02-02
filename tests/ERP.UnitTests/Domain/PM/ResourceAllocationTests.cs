using ERP.Domain.PM.Entities;

namespace ERP.UnitTests.Domain.PM;

/// <summary>
/// Unit tests for ResourceAllocation entity
/// </summary>
public class ResourceAllocationTests
{
    [Fact]
    public void Create_WithValidData_CreatesResourceAllocation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var projectId = 1L;
        var employeeId = 2L;
        var startDate = DateTime.UtcNow;
        var allocatedHours = 40m;
        var role = "Developer";

        // Act
        var allocation = ResourceAllocation.Create(
            tenantId,
            projectId,
            employeeId,
            startDate,
            allocatedHours,
            role);

        // Assert
        Assert.NotNull(allocation);
        Assert.Equal(tenantId, allocation.TenantId);
        Assert.Equal(projectId, allocation.ProjectId);
        Assert.Equal(employeeId, allocation.EmployeeId);
        Assert.Equal(startDate, allocation.StartDate);
        Assert.Equal(allocatedHours, allocation.AllocatedHoursPerWeek);
        Assert.Equal(role, allocation.Role);
        Assert.True(allocation.IsActive);
        Assert.Null(allocation.EndDate);
    }

    [Fact]
    public void Create_WithEndDate_SetsEndDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddMonths(3);

        // Act
        var allocation = ResourceAllocation.Create(
            tenantId,
            1L,
            2L,
            startDate,
            40m,
            endDate: endDate);

        // Assert
        Assert.Equal(endDate, allocation.EndDate);
    }

    [Fact]
    public void Create_WithNotes_SetsNotes()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var notes = "Important allocation notes";

        // Act
        var allocation = ResourceAllocation.Create(
            tenantId,
            1L,
            2L,
            DateTime.UtcNow,
            40m,
            notes: notes);

        // Assert
        Assert.Equal(notes, allocation.Notes);
    }

    [Fact]
    public void Create_WithZeroHours_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            ResourceAllocation.Create(
                tenantId,
                1L,
                2L,
                DateTime.UtcNow,
                0m));

        Assert.Contains("greater than zero", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeHours_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            ResourceAllocation.Create(
                tenantId,
                1L,
                2L,
                DateTime.UtcNow,
                -10m));

        Assert.Contains("greater than zero", exception.Message);
    }

    [Fact]
    public void Create_WithHoursExceeding168_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            ResourceAllocation.Create(
                tenantId,
                1L,
                2L,
                DateTime.UtcNow,
                169m));

        Assert.Contains("168", exception.Message);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            ResourceAllocation.Create(
                tenantId,
                1L,
                2L,
                startDate,
                40m,
                endDate: endDate));

        Assert.Contains("after start date", exception.Message);
    }

    [Fact]
    public void UpdateAllocation_WithValidHours_UpdatesHours()
    {
        // Arrange
        var allocation = CreateTestAllocation();
        var newHours = 30m;

        // Act
        allocation.UpdateAllocation(newHours);

        // Assert
        Assert.Equal(newHours, allocation.AllocatedHoursPerWeek);
        Assert.NotNull(allocation.ModifiedDate);
    }

    [Fact]
    public void UpdateAllocation_WithNewRole_UpdatesRole()
    {
        // Arrange
        var allocation = CreateTestAllocation();
        var newRole = "Senior Developer";

        // Act
        allocation.UpdateAllocation(40m, newRole);

        // Assert
        Assert.Equal(newRole, allocation.Role);
    }

    [Fact]
    public void UpdateAllocation_WithZeroHours_ThrowsArgumentException()
    {
        // Arrange
        var allocation = CreateTestAllocation();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => allocation.UpdateAllocation(0m));
    }

    [Fact]
    public void End_WithValidEndDate_SetsEndDateAndDeactivates()
    {
        // Arrange
        var allocation = CreateTestAllocation();
        var endDate = DateTime.UtcNow.AddDays(30);

        // Act
        allocation.End(endDate);

        // Assert
        Assert.Equal(endDate, allocation.EndDate);
        Assert.False(allocation.IsActive);
        Assert.NotNull(allocation.ModifiedDate);
    }

    [Fact]
    public void End_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var allocation = CreateTestAllocation();
        var endDate = DateTime.UtcNow.AddDays(-30);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => allocation.End(endDate));
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var allocation = CreateTestAllocation();
        allocation.Deactivate();
        Assert.False(allocation.IsActive);

        // Act
        allocation.Activate();

        // Assert
        Assert.True(allocation.IsActive);
        Assert.Null(allocation.EndDate);
        Assert.NotNull(allocation.ModifiedDate);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var allocation = CreateTestAllocation();
        Assert.True(allocation.IsActive);

        // Act
        allocation.Deactivate();

        // Assert
        Assert.False(allocation.IsActive);
        Assert.NotNull(allocation.ModifiedDate);
    }

    private ResourceAllocation CreateTestAllocation()
    {
        return ResourceAllocation.Create(
            Guid.NewGuid(),
            projectId: 1L,
            employeeId: 2L,
            startDate: DateTime.UtcNow,
            allocatedHoursPerWeek: 40m,
            role: "Developer");
    }
}
