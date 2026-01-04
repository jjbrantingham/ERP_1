using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.PM.Commands;
using ERP.Domain.CRM.Entities;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.CRM.Repositories;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
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
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentTenantService> _currentTenantMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CreateProjectCommandHandler _handler;

    private readonly Guid _testTenantId = Guid.NewGuid();
    private readonly Guid _testUserId = Guid.NewGuid();

    public CreateProjectCommandHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _clientRepositoryMock = new Mock<IClientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentTenantMock = new Mock<ICurrentTenantService>();
        _currentUserMock = new Mock<ICurrentUserService>();

        // Setup default tenant and user
        _currentTenantMock.Setup(x => x.TenantId).Returns(_testTenantId);
        _currentTenantMock.Setup(x => x.TenantName).Returns("Test Tenant");
        _currentUserMock.Setup(x => x.UserId).Returns(_testUserId);
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);

        _handler = new CreateProjectCommandHandler(
            _projectRepositoryMock.Object,
            _clientRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentTenantMock.Object,
            _currentUserMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProject()
    {
        // Arrange
        var client = CreateTestClient();
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = client.Id
        };

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _projectRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Project>(p =>
                p.Name == "Test Project" &&
                p.Description == "Test Description" &&
                p.Type == ProjectType.Billable &&
                p.ClientId == client.Id &&
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
    public async Task Handle_ClientNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = 999
        };

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Client*");

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ClientBelongsToAnotherTenant_ShouldThrowNotFoundException()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var client = Client.Create(
            otherTenantId, // Different tenant
            "Other Tenant Client",
            ClientType.Corporate,
            null, null,
            Email.Create("other@example.com"),
            null, null
        );

        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = client.Id
        };

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Client*");

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var client = CreateTestClient();
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = client.Id
        };

        // Setup unauthenticated user
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

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
        var client = CreateTestClient();
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = client.Id
        };

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // First call returns true (collision), second returns false (unique)
        _projectRepositoryMock
            .SetupSequence(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);

        // Verify ExistsAsync was called at least twice (once for collision, once for unique)
        _projectRepositoryMock.Verify(
            x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeast(2)
        );
    }

    /// <summary>
    /// Helper method to create a test client.
    /// </summary>
    private Client CreateTestClient()
    {
        return Client.Create(
            _testTenantId,
            "Test Client",
            ClientType.Corporate,
            null,
            null,
            Email.Create("client@example.com"),
            null,
            null
        );
    }
}
