using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;

namespace ERP.UnitTests.Domain.PM;

/// <summary>
/// Unit tests for WBSItem entity
/// </summary>
public class WBSItemTests
{
    [Fact]
    public void Create_WithValidData_CreatesWBSItem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var projectId = 1L;
        var code = "1.0";
        var name = "Phase 1";
        var level = 1;
        var displayOrder = 1;

        // Act
        var wbsItem = WBSItem.Create(
            tenantId,
            projectId,
            code,
            name,
            level,
            displayOrder);

        // Assert
        Assert.NotNull(wbsItem);
        Assert.Equal(tenantId, wbsItem.TenantId);
        Assert.Equal(projectId, wbsItem.ProjectId);
        Assert.Equal(code, wbsItem.Code);
        Assert.Equal(name, wbsItem.Name);
        Assert.Equal(level, wbsItem.Level);
        Assert.Equal(displayOrder, wbsItem.DisplayOrder);
        Assert.True(wbsItem.IsActive);
        Assert.Equal(0m, wbsItem.PercentComplete);
    }

    [Fact]
    public void Create_WithParentId_SetsParentId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var parentId = 10L;

        // Act
        var wbsItem = WBSItem.Create(
            tenantId,
            1L,
            "1.1",
            "Sub-Phase",
            2,
            1,
            parentId);

        // Assert
        Assert.Equal(parentId, wbsItem.ParentId);
    }

    [Fact]
    public void Create_WithOptionalFields_SetsFieldsCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var description = "First phase of the project";
        var budget = new Money(25000m, "USD");
        var estimatedHours = 160m;
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddMonths(1);

        // Act
        var wbsItem = WBSItem.Create(
            tenantId,
            1L,
            "1.0",
            "Phase 1",
            1,
            1,
            description: description,
            budget: budget,
            estimatedHours: estimatedHours,
            startDate: startDate,
            endDate: endDate);

        // Assert
        Assert.Equal(description, wbsItem.Description);
        Assert.Equal(budget, wbsItem.Budget);
        Assert.Equal(estimatedHours, wbsItem.EstimatedHours);
        Assert.Equal(startDate, wbsItem.StartDate);
        Assert.Equal(endDate, wbsItem.EndDate);
    }

    [Fact]
    public void Create_TrimsWhitespace_FromStringProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var wbsItem = WBSItem.Create(
            tenantId,
            1L,
            "  1.0  ",
            "  Phase 1  ",
            1,
            1,
            description: "  Description  ");

        // Assert
        Assert.Equal("1.0", wbsItem.Code);
        Assert.Equal("Phase 1", wbsItem.Name);
        Assert.Equal("Description", wbsItem.Description);
    }

    [Fact]
    public void UpdateProgress_WithValidPercentComplete_UpdatesProgress()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();
        var percentComplete = 50m;

        // Act
        wbsItem.UpdateProgress(percentComplete);

        // Assert
        Assert.Equal(percentComplete, wbsItem.PercentComplete);
        Assert.NotNull(wbsItem.ModifiedDate);
    }

    [Fact]
    public void UpdateProgress_WithActualHours_UpdatesActualHours()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();
        var actualHours = 80m;

        // Act
        wbsItem.UpdateProgress(50m, actualHours);

        // Assert
        Assert.Equal(actualHours, wbsItem.ActualHours);
    }

    [Fact]
    public void UpdateProgress_ClampsPercentComplete_ToValidRange()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();

        // Act
        wbsItem.UpdateProgress(150m);

        // Assert
        Assert.Equal(100m, wbsItem.PercentComplete);

        // Act
        wbsItem.UpdateProgress(-50m);

        // Assert
        Assert.Equal(0m, wbsItem.PercentComplete);
    }

    [Fact]
    public void Update_WithValidData_UpdatesWBSItem()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();
        var newName = "Updated Phase";
        var newDescription = "Updated description";
        var newDisplayOrder = 5;
        var newBudget = new Money(50000m, "USD");
        var newEstimatedHours = 200m;
        var newStartDate = DateTime.UtcNow.AddDays(10);
        var newEndDate = DateTime.UtcNow.AddMonths(3);

        // Act
        wbsItem.Update(
            newName,
            newDescription,
            newDisplayOrder,
            newBudget,
            newEstimatedHours,
            newStartDate,
            newEndDate);

        // Assert
        Assert.Equal(newName, wbsItem.Name);
        Assert.Equal(newDescription, wbsItem.Description);
        Assert.Equal(newDisplayOrder, wbsItem.DisplayOrder);
        Assert.Equal(newBudget, wbsItem.Budget);
        Assert.Equal(newEstimatedHours, wbsItem.EstimatedHours);
        Assert.Equal(newStartDate, wbsItem.StartDate);
        Assert.Equal(newEndDate, wbsItem.EndDate);
        Assert.NotNull(wbsItem.ModifiedDate);
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            wbsItem.Update("", null, 1, null, null, null, null));
    }

    [Fact]
    public void Update_WithWhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            wbsItem.Update("   ", null, 1, null, null, null, null));
    }

    [Fact]
    public void Update_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            wbsItem.Update("Valid Name", null, 1, null, null, startDate, endDate));
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();
        wbsItem.Deactivate();
        Assert.False(wbsItem.IsActive);

        // Act
        wbsItem.Activate();

        // Assert
        Assert.True(wbsItem.IsActive);
        Assert.NotNull(wbsItem.ModifiedDate);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var wbsItem = CreateTestWBSItem();
        Assert.True(wbsItem.IsActive);

        // Act
        wbsItem.Deactivate();

        // Assert
        Assert.False(wbsItem.IsActive);
        Assert.NotNull(wbsItem.ModifiedDate);
    }

    private WBSItem CreateTestWBSItem()
    {
        return WBSItem.Create(
            Guid.NewGuid(),
            projectId: 1L,
            code: "1.0",
            name: "Phase 1",
            level: 1,
            displayOrder: 1,
            description: "Test phase",
            budget: new Money(25000m, "USD"),
            estimatedHours: 160m);
    }
}
