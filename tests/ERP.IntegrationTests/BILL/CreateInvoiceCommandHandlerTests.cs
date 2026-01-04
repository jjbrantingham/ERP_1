using ERP.Application.BILL.Commands;
using ERP.Application.Common.Exceptions;
using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.CRM.Entities;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.IntegrationTests.Infrastructure;
using FluentAssertions;
using MediatR;

namespace ERP.IntegrationTests.BILL;

/// <summary>
/// Integration tests for CreateInvoiceCommandHandler.
/// CRITICAL: Financial data integrity is paramount for this ERP system.
/// </summary>
public class CreateInvoiceCommandHandlerTests : IntegrationTestBase
{
    private readonly IMediator _mediator;
    private readonly IInvoiceRepository _invoiceRepository;

    public CreateInvoiceCommandHandlerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _mediator = GetService<IMediator>();
        _invoiceRepository = GetService<IInvoiceRepository>();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateInvoice()
    {
        // Arrange
        var (project, client) = await CreateTestProjectAsync();

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

        // Act
        var invoiceId = await _mediator.Send(command);

        // Assert
        invoiceId.Should().BeGreaterThan(0);

        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        invoice.Should().NotBeNull();
        invoice!.ProjectId.Should().Be(project.Id);
        invoice.ClientId.Should().Be(client.Id);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.TotalAmount.Should().NotBeNull();
        invoice.TotalAmount!.Amount.Should().Be(1500.00m);
        invoice.TenantId.Should().Be(TestAuthenticationHelper.TestTenantId);
    }

    [Fact]
    public async Task Handle_InvalidProject_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ProjectId = 999999, // Non-existent project
            ClientId = 1,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = BillingMode.TimeAndMaterials,
            LineItems = new List<CreateInvoiceCommand.InvoiceLineItemDto>()
        };

        // Act
        Func<Task> act = async () => await _mediator.Send(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Project*");
    }

    [Fact]
    public async Task Handle_MultipleLineItems_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var (project, client) = await CreateTestProjectAsync();

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
                },
                new()
                {
                    Description = "Development Services",
                    Quantity = 20,
                    UnitPrice = 125.00m,
                    Amount = 2500.00m
                }
            }
        };

        // Act
        var invoiceId = await _mediator.Send(command);

        // Assert
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        invoice.Should().NotBeNull();
        invoice!.TotalAmount!.Amount.Should().Be(4000.00m); // 1500 + 2500
    }

    [Fact]
    public async Task Handle_InvoiceNumber_ShouldBeUnique()
    {
        // Arrange
        var (project, client) = await CreateTestProjectAsync();

        var command1 = new CreateInvoiceCommand
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
                    Description = "Services 1",
                    Quantity = 1,
                    UnitPrice = 100.00m,
                    Amount = 100.00m
                }
            }
        };

        var command2 = new CreateInvoiceCommand
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
                    Description = "Services 2",
                    Quantity = 1,
                    UnitPrice = 200.00m,
                    Amount = 200.00m
                }
            }
        };

        // Act
        var invoiceId1 = await _mediator.Send(command1);
        var invoiceId2 = await _mediator.Send(command2);

        // Assert
        var invoice1 = await _invoiceRepository.GetByIdAsync(invoiceId1);
        var invoice2 = await _invoiceRepository.GetByIdAsync(invoiceId2);

        invoice1!.InvoiceNumber.Should().NotBe(invoice2!.InvoiceNumber);
    }

    /// <summary>
    /// Helper method to create a test project and client.
    /// </summary>
    private async Task<(Project project, Client client)> CreateTestProjectAsync()
    {
        var client = Client.Create(
            TestAuthenticationHelper.TestTenantId,
            "Test Client",
            ClientType.Corporate,
            null,
            null,
            Email.Create("client@example.com"),
            null,
            null
        );

        var project = Project.Create(
            TestAuthenticationHelper.TestTenantId,
            "PRJ-TEST",
            "Test Project",
            "Test Description",
            ProjectType.Billable,
            client.Id
        );

        DbContext.Set<Client>().Add(client);
        DbContext.Set<Project>().Add(project);
        await DbContext.SaveChangesAsync();

        return (project, client);
    }
}
