using ERP.Application.BILL.Commands;
using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.ValueObjects;
using ProjectBillingMode = ERP.Domain.PM.Enums.BillingMode;
using ProjectType = ERP.Domain.PM.Enums.ProjectType;
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
        var invoiceId = await _mediator.Send(command);

        // Assert
        invoiceId.Should().BeGreaterThan(0);

        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        invoice.Should().NotBeNull();
        invoice!.ProjectId.Should().Be(project.Id);
        invoice.ClientId.Should().Be(client.Id);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.TenantId.Should().Be(TestAuthenticationHelper.TestTenantId);
    }

    [Fact]
    public async Task Handle_MultipleLineItems_ShouldCreateInvoiceWithAllItems()
    {
        // Arrange
        var (project, client) = await CreateTestProjectAsync();

        var command = new CreateInvoiceCommand
        {
            ProjectId = project.Id,
            ClientId = client.Id,
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
                },
                new()
                {
                    Description = "Development Services",
                    Quantity = 20,
                    UnitPrice = 125.00m
                }
            }
        };

        // Act
        var invoiceId = await _mediator.Send(command);

        // Assert
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        invoice.Should().NotBeNull();
        invoice!.LineItems.Should().HaveCount(2);
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
            BillingMode = "TimeAndMaterials",
            Currency = "USD",
            LineItems = new List<InvoiceLineItemCommand>
            {
                new()
                {
                    Description = "Services 1",
                    Quantity = 1,
                    UnitPrice = 100.00m
                }
            }
        };

        var command2 = new CreateInvoiceCommand
        {
            ProjectId = project.Id,
            ClientId = client.Id,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            BillingMode = "TimeAndMaterials",
            Currency = "USD",
            LineItems = new List<InvoiceLineItemCommand>
            {
                new()
                {
                    Description = "Services 2",
                    Quantity = 1,
                    UnitPrice = 200.00m
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
        var clientNumber = Client.GenerateClientNumber();
        var client = Client.Create(
            TestAuthenticationHelper.TestTenantId,
            clientNumber,
            "Test Client",
            ClientType.Corporate,
            primaryEmail: new Email("client@example.com")
        );

        DbContext.Set<Client>().Add(client);
        await DbContext.SaveChangesAsync();

        var projectNumber = ProjectNumber.Generate();
        var project = Project.Create(
            TestAuthenticationHelper.TestTenantId,
            projectNumber,
            client.Id,
            "Test Project",
            ProjectType.Billable,
            ProjectBillingMode.TimeAndMaterials,
            DateTime.UtcNow,
            "Test Description"
        );

        DbContext.Set<Project>().Add(project);
        await DbContext.SaveChangesAsync();

        return (project, client);
    }
}
