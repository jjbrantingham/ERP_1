using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.PM.Commands;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using ERP.Domain.PM.ValueObjects;
using FluentAssertions;
using Moq;

namespace ERP.UnitTests.Application.PM;

/// <summary>
/// Unit tests for CreateProjectCommandHandler.
/// Tests business logic, validation, and error scenarios using mocks.
/// </summary>
public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentTenantService> _currentTenantMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CreateProjectCommandHandler _handler;

    private readonly Guid _testTenantId = Guid.NewGuid();

    public CreateProjectCommandHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentTenantMock = new Mock<ICurrentTenantService>();
        _currentUserMock = new Mock<ICurrentUserService>();

        // Setup default tenant and user
        _currentTenantMock.Setup(x => x.TenantId).Returns(_testTenantId);
        _currentTenantMock.Setup(x => x.TenantName).Returns("Test Tenant");
        _currentUserMock.Setup(x => x.UserId).Returns(1L);
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);

        _handler = new CreateProjectCommandHandler(
            _projectRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentTenantMock.Object,
            _currentUserMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProject()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            ClientId = 1L,
            Name = "Test Project",
            Description = "Test Description",
            ProjectType = ProjectType.Billable,
            BillingMode = BillingMode.TimeAndMaterials,
            StartDate = DateTime.UtcNow
        };

        _projectRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<ProjectNumber>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Project>(p =>
                p.Name == "Test Project" &&
                p.Description == "Test Description" &&
                p.ProjectType == ProjectType.Billable &&
                p.ClientId == 1L &&
                p.TenantId == _testTenantId
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            ClientId = 1L,
            Name = "Test Project",
            Description = "Test Description",
            ProjectType = ProjectType.Billable,
            BillingMode = BillingMode.TimeAndMaterials,
            StartDate = DateTime.UtcNow
        };

        // Setup unauthenticated user
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ProjectNumberCollision_ShouldGenerateNewNumber()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            ClientId = 1L,
            Name = "Test Project",
            Description = "Test Description",
            ProjectType = ProjectType.Billable,
            BillingMode = BillingMode.TimeAndMaterials,
            StartDate = DateTime.UtcNow
        };

        // First call returns true (collision), second returns false (unique)
        _projectRepositoryMock
            .SetupSequence(x => x.ExistsAsync(It.IsAny<ProjectNumber>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);

        // Verify ExistsAsync was called at least twice (once for collision, once for unique)
        _projectRepositoryMock.Verify(
            x => x.ExistsAsync(It.IsAny<ProjectNumber>(), It.IsAny<CancellationToken>()),
            Times.AtLeast(2)
        );
    }

    [Fact]
    public async Task Handle_WithBudget_ShouldCreateProjectWithBudget()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            ClientId = 1L,
            Name = "Budgeted Project",
            Description = "Project with budget",
            ProjectType = ProjectType.Billable,
            BillingMode = BillingMode.FixedPrice,
            StartDate = DateTime.UtcNow,
            BudgetAmount = 50000.00m,
            BudgetCurrency = "USD"
        };

        _projectRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<ProjectNumber>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Project>(p =>
                p.Name == "Budgeted Project" &&
                p.Budget != null &&
                p.Budget.Amount == 50000.00m &&
                p.Budget.Currency == "USD"
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData(ProjectType.Billable, BillingMode.TimeAndMaterials)]
    [InlineData(ProjectType.Overhead, BillingMode.NonBillable)]
    [InlineData(ProjectType.Internal, BillingMode.NonBillable)]
    public async Task Handle_VariousProjectTypes_ShouldCreateProject(ProjectType projectType, BillingMode billingMode)
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            ClientId = 1L,
            Name = "Test Project",
            ProjectType = projectType,
            BillingMode = billingMode,
            StartDate = DateTime.UtcNow
        };

        _projectRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<ProjectNumber>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Project>(p =>
                p.ProjectType == projectType
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
