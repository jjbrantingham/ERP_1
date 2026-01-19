using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Events;
using ERP.Domain.PM.ValueObjects;

namespace ERP.UnitTests.Domain.PM;

/// <summary>
/// Unit tests for Project entity
/// CRITICAL: Tests project lifecycle and business rules
/// </summary>
public class ProjectTests
{
    [Fact]
    public void Create_WithValidData_CreatesProject()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var projectNumber = new ProjectNumber("PRJ-2024-001");
        var clientId = 1L;
        var name = "Website Redesign";
        var projectType = ProjectType.Billable;
        var billingMode = BillingMode.TimeAndMaterials;
        var startDate = DateTime.UtcNow;

        // Act
        var project = Project.Create(
            tenantId,
            projectNumber,
            clientId,
            name,
            projectType,
            billingMode,
            startDate);

        // Assert
        Assert.NotNull(project);
        Assert.Equal(tenantId, project.TenantId);
        Assert.Equal(projectNumber, project.ProjectNumber);
        Assert.Equal(clientId, project.ClientId);
        Assert.Equal(name, project.Name);
        Assert.Equal(projectType, project.ProjectType);
        Assert.Equal(billingMode, project.BillingMode);
        Assert.Equal(ProjectStatus.Planning, project.Status);
        Assert.Equal(startDate, project.StartDate);
        Assert.True(project.IsActive);
        Assert.Single(project.DomainEvents);
        Assert.IsType<ProjectCreatedEvent>(project.DomainEvents.First());
    }

    [Fact]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var projectNumber = new ProjectNumber("PRJ-2024-001");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Project.Create(
                tenantId,
                projectNumber,
                1L,
                null!,
                ProjectType.Billable,
                BillingMode.TimeAndMaterials,
                DateTime.UtcNow));

        Assert.Contains("Project name is required", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var projectNumber = new ProjectNumber("PRJ-2024-001");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Project.Create(
                tenantId,
                projectNumber,
                1L,
                "   ",
                ProjectType.Billable,
                BillingMode.TimeAndMaterials,
                DateTime.UtcNow));

        Assert.Contains("Project name is required", exception.Message);
    }

    [Fact]
    public void Create_WithOptionalParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var projectNumber = new ProjectNumber("PRJ-2024-001");
        var description = "Complete website redesign project";
        var endDate = DateTime.UtcNow.AddMonths(3);
        var budget = new Money(100000m, "USD");
        var projectManagerId = 5L;
        var notes = "Important client project";

        // Act
        var project = Project.Create(
            tenantId,
            projectNumber,
            1L,
            "Website Redesign",
            ProjectType.Billable,
            BillingMode.FixedPrice,
            DateTime.UtcNow,
            description,
            endDate,
            budget,
            projectManagerId,
            notes);

        // Assert
        Assert.Equal(description, project.Description);
        Assert.Equal(endDate, project.EndDate);
        Assert.Equal(budget, project.Budget);
        Assert.Equal(projectManagerId, project.ProjectManagerId);
        Assert.Equal(notes, project.Notes);
    }

    [Fact]
    public void UpdateInfo_WithValidData_UpdatesProjectInfo()
    {
        // Arrange
        var project = CreateTestProject();
        var newName = "Updated Project Name";
        var newDescription = "Updated description";
        var newBudget = new Money(150000m, "USD");

        // Act
        project.UpdateInfo(
            newName,
            ProjectType.Overhead,
            BillingMode.FixedPrice,
            DateTime.UtcNow.AddDays(10),
            newDescription,
            DateTime.UtcNow.AddMonths(6),
            newBudget,
            10L,
            "Updated notes");

        // Assert
        Assert.Equal(newName, project.Name);
        Assert.Equal(newDescription, project.Description);
        Assert.Equal(ProjectType.Overhead, project.ProjectType);
        Assert.Equal(BillingMode.FixedPrice, project.BillingMode);
        Assert.Equal(newBudget, project.Budget);
        Assert.Equal(10L, project.ProjectManagerId);
        Assert.Equal("Updated notes", project.Notes);
        Assert.NotNull(project.ModifiedDate);
    }

    [Fact]
    public void UpdateInfo_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var project = CreateTestProject();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => project.UpdateInfo(
                null!,
                ProjectType.Billable,
                BillingMode.TimeAndMaterials,
                DateTime.UtcNow));

        Assert.Contains("Project name is required", exception.Message);
    }

    [Fact]
    public void ChangeStatus_ToActive_SetsActualStartDate()
    {
        // Arrange
        var project = CreateTestProject();
        Assert.Null(project.ActualStartDate);

        // Act
        project.ChangeStatus(ProjectStatus.Active);

        // Assert
        Assert.Equal(ProjectStatus.Active, project.Status);
        Assert.NotNull(project.ActualStartDate);
        Assert.NotNull(project.ModifiedDate);
        Assert.Contains(project.DomainEvents, e => e is ProjectStatusChangedEvent);
    }

    [Fact]
    public void ChangeStatus_ToCompleted_SetsActualEndDate()
    {
        // Arrange
        var project = CreateTestProject();
        project.ChangeStatus(ProjectStatus.Active);
        project.DomainEvents.Clear(); // Clear previous events
        Assert.Null(project.ActualEndDate);

        // Act
        project.ChangeStatus(ProjectStatus.Completed);

        // Assert
        Assert.Equal(ProjectStatus.Completed, project.Status);
        Assert.NotNull(project.ActualEndDate);
        Assert.Single(project.DomainEvents);

        var statusEvent = project.DomainEvents.First() as ProjectStatusChangedEvent;
        Assert.NotNull(statusEvent);
        Assert.Equal(ProjectStatus.Active, statusEvent.OldStatus);
        Assert.Equal(ProjectStatus.Completed, statusEvent.NewStatus);
    }

    [Fact]
    public void ChangeStatus_ToClosed_SetsActualEndDate()
    {
        // Arrange
        var project = CreateTestProject();
        project.ChangeStatus(ProjectStatus.Active);
        Assert.Null(project.ActualEndDate);

        // Act
        project.ChangeStatus(ProjectStatus.Closed);

        // Assert
        Assert.Equal(ProjectStatus.Closed, project.Status);
        Assert.NotNull(project.ActualEndDate);
    }

    [Fact]
    public void ChangeStatus_WhenAlreadyActive_DoesNotChangeActualStartDate()
    {
        // Arrange
        var project = CreateTestProject();
        project.ChangeStatus(ProjectStatus.Active);
        var originalStartDate = project.ActualStartDate;

        // Act
        project.ChangeStatus(ProjectStatus.OnHold);
        project.ChangeStatus(ProjectStatus.Active);

        // Assert
        Assert.Equal(originalStartDate, project.ActualStartDate);
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var project = CreateTestProject();
        project.Deactivate();
        Assert.False(project.IsActive);

        // Act
        project.Activate();

        // Assert
        Assert.True(project.IsActive);
        Assert.NotNull(project.ModifiedDate);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var project = CreateTestProject();
        Assert.True(project.IsActive);

        // Act
        project.Deactivate();

        // Assert
        Assert.False(project.IsActive);
        Assert.NotNull(project.ModifiedDate);
    }

    [Fact]
    public void UpdateActualCost_UpdatesCostAndModifiedDate()
    {
        // Arrange
        var project = CreateTestProject();
        var actualCost = new Money(75000m, "USD");

        // Act
        project.UpdateActualCost(actualCost);

        // Assert
        Assert.Equal(actualCost, project.ActualCost);
        Assert.NotNull(project.ModifiedDate);
    }

    [Theory]
    [InlineData(ProjectType.Billable, BillingMode.TimeAndMaterials)]
    [InlineData(ProjectType.Billable, BillingMode.FixedPrice)]
    [InlineData(ProjectType.Billable, BillingMode.Milestone)]
    [InlineData(ProjectType.Overhead, BillingMode.NonBillable)]
    [InlineData(ProjectType.Proposal, BillingMode.NonBillable)]
    public void Create_WithVariousProjectTypesAndBillingModes_CreatesSuccessfully(
        ProjectType projectType,
        BillingMode billingMode)
    {
        // Arrange & Act
        var project = Project.Create(
            Guid.NewGuid(),
            new ProjectNumber("PRJ-2024-001"),
            1L,
            "Test Project",
            projectType,
            billingMode,
            DateTime.UtcNow);

        // Assert
        Assert.Equal(projectType, project.ProjectType);
        Assert.Equal(billingMode, project.BillingMode);
    }

    [Fact]
    public void Create_TrimsWhitespaceFromStringProperties()
    {
        // Arrange
        var name = "  Test Project  ";
        var description = "  Test Description  ";
        var notes = "  Test Notes  ";

        // Act
        var project = Project.Create(
            Guid.NewGuid(),
            new ProjectNumber("PRJ-2024-001"),
            1L,
            name,
            ProjectType.Billable,
            BillingMode.TimeAndMaterials,
            DateTime.UtcNow,
            description,
            null,
            null,
            null,
            notes);

        // Assert
        Assert.Equal("Test Project", project.Name);
        Assert.Equal("Test Description", project.Description);
        Assert.Equal("Test Notes", project.Notes);
    }

    [Fact]
    public void UpdateInfo_TrimsWhitespaceFromStringProperties()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        project.UpdateInfo(
            "  Updated Name  ",
            ProjectType.Billable,
            BillingMode.TimeAndMaterials,
            DateTime.UtcNow,
            "  Updated Description  ",
            null,
            null,
            null,
            "  Updated Notes  ");

        // Assert
        Assert.Equal("Updated Name", project.Name);
        Assert.Equal("Updated Description", project.Description);
        Assert.Equal("Updated Notes", project.Notes);
    }

    // Helper method to create test projects
    private Project CreateTestProject()
    {
        return Project.Create(
            Guid.NewGuid(),
            new ProjectNumber("PRJ-2024-001"),
            clientId: 1L,
            "Test Project",
            ProjectType.Billable,
            BillingMode.TimeAndMaterials,
            DateTime.UtcNow,
            "Test Description",
            DateTime.UtcNow.AddMonths(6),
            new Money(100000m, "USD"),
            projectManagerId: 1L,
            "Test Notes");
    }
}
