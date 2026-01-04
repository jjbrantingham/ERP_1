using ERP.Application.BILL.Commands;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Repositories;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using FluentAssertions;
using Moq;

namespace ERP.UnitTests.Application.BILL;

/// <summary>
/// Unit tests for CreateInvoiceCommandHandler.
/// CRITICAL: Financial calculations must be precise and correct.
/// </summary>
public class CreateInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentTenantService> _currentTenantMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CreateInvoiceCommandHandler _handler;

    private readonly Guid _testTenantId = Guid.NewGuid();
    private readonly Guid _testUserId = Guid.NewGuid();

    public CreateInvoiceCommandHandlerTests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _clientRepositoryMock = new Mock<IClientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentTenantMock = new Mock<ICurrentTenantService>();
        _currentUserMock = new Mock<ICurrentUserService>();

        // Setup default tenant and user
        _currentTenantMock.Setup(x => x.TenantId).Returns(_testTenantId);
        _currentUserMock.Setup(x => x.UserId).Returns(_testUserId);
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);

        _handler = new CreateInvoiceCommandHandler(
            _invoiceRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _clientRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentTenantMock.Object,
            _currentUserMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateInvoice()
    {
        // Arrange
        var (project, client) = CreateTestProjectAndClient();
        var command = new CreateInvoiceCommand
        {
            ProjectId = project.Id,
            ClientId = client.Id,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = BillingMode.TimeAndMaterials,
            LineItems = new List<CreateInvoiceCommand.InvoiceLineItemDto>
            {
                new()
                {
                    Description = "Consulting Services",
                    Quantity = 10,
                    UnitPrice = 150.00m,
                    Amount = 1500.00m
                }
            }
        };

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _invoiceRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Invoice>(inv =>
                inv.ProjectId == project.Id &&
                inv.ClientId == client.Id &&
                inv.BillingMode == BillingMode.TimeAndMaterials &&
                inv.Status == InvoiceStatus.Draft &&
                inv.TenantId == _testTenantId &&
                inv.TotalAmount != null &&
                inv.TotalAmount.Amount == 1500.00m
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_MultipleLineItems_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var (project, client) = CreateTestProjectAndClient();
        var command = new CreateInvoiceCommand
        {
            ProjectId = project.Id,
            ClientId = client.Id,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = BillingMode.TimeAndMaterials,
            LineItems = new List<CreateInvoiceCommand.InvoiceLineItemDto>
            {
                new()
                {
                    Description = "Consulting",
                    Quantity = 10,
                    UnitPrice = 150.00m,
                    Amount = 1500.00m
                },
                new()
                {
                    Description = "Development",
                    Quantity = 20,
                    UnitPrice = 125.00m,
                    Amount = 2500.00m
                },
                new()
                {
                    Description = "Testing",
                    Quantity = 5,
                    UnitPrice = 100.00m,
                    Amount = 500.00m
                }
            }
        };

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _invoiceRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Invoice>(inv =>
                inv.TotalAmount != null &&
                inv.TotalAmount.Amount == 4500.00m // 1500 + 2500 + 500
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ProjectNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ProjectId = 999,
            ClientId = 1,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = BillingMode.TimeAndMaterials,
            LineItems = new List<CreateInvoiceCommand.InvoiceLineItemDto>()
        };

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Project*");

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ClientNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var (project, _) = CreateTestProjectAndClient();
        var command = new CreateInvoiceCommand
        {
            ProjectId = project.Id,
            ClientId = 999,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = BillingMode.TimeAndMaterials,
            LineItems = new List<CreateInvoiceCommand.InvoiceLineItemDto>()
        };

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Client*");
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var (project, client) = CreateTestProjectAndClient();
        var command = new CreateInvoiceCommand
        {
            ProjectId = project.Id,
            ClientId = client.Id,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = BillingMode.TimeAndMaterials,
            LineItems = new List<CreateInvoiceCommand.InvoiceLineItemDto>()
        };

        // Setup unauthenticated user
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_EmptyLineItems_ShouldCreateInvoiceWithZeroTotal()
    {
        // Arrange
        var (project, client) = CreateTestProjectAndClient();
        var command = new CreateInvoiceCommand
        {
            ProjectId = project.Id,
            ClientId = client.Id,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = BillingMode.FixedPrice,
            LineItems = new List<CreateInvoiceCommand.InvoiceLineItemDto>()
        };

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _invoiceRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Invoice>(inv =>
                inv.TotalAmount != null &&
                inv.TotalAmount.Amount == 0.00m
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    /// <summary>
    /// Helper method to create a test project and client.
    /// </summary>
    private (Project project, Client client) CreateTestProjectAndClient()
    {
        var client = Client.Create(
            _testTenantId,
            "Test Client",
            ClientType.Corporate,
            null, null,
            Email.Create("client@example.com"),
            null, null
        );

        var project = Project.Create(
            _testTenantId,
            "PRJ-TEST",
            "Test Project",
            "Test Description",
            ProjectType.Billable,
            client.Id
        );

        return (project, client);
    }
}
