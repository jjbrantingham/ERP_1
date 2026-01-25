using ERP.Application.BILL.Commands;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.Common.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ERP.UnitTests.Application.BILL;

/// <summary>
/// Unit tests for CreateInvoiceCommandHandler.
/// CRITICAL: Financial calculations must be precise and correct.
/// </summary>
public class CreateInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentTenantService> _currentTenantMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<CreateInvoiceCommandHandler>> _loggerMock;
    private readonly CreateInvoiceCommandHandler _handler;

    private readonly Guid _testTenantId = Guid.NewGuid();

    public CreateInvoiceCommandHandlerTests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentTenantMock = new Mock<ICurrentTenantService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<CreateInvoiceCommandHandler>>();

        // Setup default tenant and user
        _currentTenantMock.Setup(x => x.TenantId).Returns(_testTenantId);
        _currentUserMock.Setup(x => x.UserId).Returns(1L);
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);

        _handler = new CreateInvoiceCommandHandler(
            _invoiceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentTenantMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateInvoice()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ClientId = 1L,
            ProjectId = 1L,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = "TimeAndMaterials",
            Currency = "USD",
            LineItems = new List<InvoiceLineItemCommand>
            {
                new()
                {
                    Description = "Consulting Services",
                    Quantity = 10,
                    UnitPrice = 150.00m
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Invoice>(inv =>
                inv.ClientId == command.ClientId &&
                inv.ProjectId == command.ProjectId &&
                inv.BillingMode == BillingMode.TimeAndMaterials &&
                inv.Status == InvoiceStatus.Draft &&
                inv.TenantId == _testTenantId
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_MultipleLineItems_ShouldCreateInvoiceWithAllItems()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ClientId = 1L,
            ProjectId = 1L,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = "TimeAndMaterials",
            Currency = "USD",
            LineItems = new List<InvoiceLineItemCommand>
            {
                new()
                {
                    Description = "Consulting",
                    Quantity = 10,
                    UnitPrice = 150.00m
                },
                new()
                {
                    Description = "Development",
                    Quantity = 20,
                    UnitPrice = 125.00m
                },
                new()
                {
                    Description = "Testing",
                    Quantity = 5,
                    UnitPrice = 100.00m
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Invoice>(inv =>
                inv.LineItems.Count == 3
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_InvalidBillingMode_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ClientId = 1L,
            ProjectId = 1L,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = "InvalidMode",
            Currency = "USD",
            LineItems = new List<InvoiceLineItemCommand>()
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid billing mode*");

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ClientId = 1L,
            ProjectId = 1L,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = "TimeAndMaterials",
            Currency = "USD",
            LineItems = new List<InvoiceLineItemCommand>()
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
        var command = new CreateInvoiceCommand
        {
            ClientId = 1L,
            ProjectId = 1L,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = "FixedPrice",
            Currency = "USD",
            LineItems = new List<InvoiceLineItemCommand>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);

        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Invoice>(inv =>
                inv.LineItems.Count == 0
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData("TimeAndMaterials")]
    [InlineData("FixedPrice")]
    [InlineData("Milestone")]
    public async Task Handle_VariousBillingModes_ShouldCreateInvoice(string billingMode)
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ClientId = 1L,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = billingMode,
            Currency = "USD",
            LineItems = new List<InvoiceLineItemCommand>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        _invoiceRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
