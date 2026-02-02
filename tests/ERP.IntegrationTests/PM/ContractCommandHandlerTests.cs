using ERP.Application.PM.Commands;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using ERP.IntegrationTests.Infrastructure;
using FluentAssertions;
using MediatR;

namespace ERP.IntegrationTests.PM;

/// <summary>
/// Integration tests for Contract command handlers.
/// </summary>
public class ContractCommandHandlerTests : IntegrationTestBase
{
    private readonly IMediator _mediator;
    private readonly IContractRepository _contractRepository;
    private readonly IProjectRepository _projectRepository;

    public ContractCommandHandlerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _mediator = GetService<IMediator>();
        _contractRepository = GetService<IContractRepository>();
        _projectRepository = GetService<IProjectRepository>();
    }

    [Fact]
    public async Task CreateContract_ValidCommand_ShouldCreateContract()
    {
        // Arrange
        var project = await CreateTestProjectAsync();

        var command = new CreateContractCommand
        {
            ProjectId = project.Id,
            ContractNumber = $"CON-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20),
            ContractType = ContractType.FixedPrice,
            Title = "Test Contract",
            Description = "Test Description",
            ContractValueAmount = 50000m,
            ContractValueCurrency = "USD",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var contractId = await _mediator.Send(command);

        // Assert
        contractId.Should().BeGreaterThan(0);

        var contract = await _contractRepository.GetByIdAsync(contractId);
        contract.Should().NotBeNull();
        contract!.ProjectId.Should().Be(project.Id);
        contract.ContractType.Should().Be(ContractType.FixedPrice);
        contract.Title.Should().Be("Test Contract");
        contract.ContractValue.Should().NotBeNull();
        contract.ContractValue!.Amount.Should().Be(50000m);
        contract.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateContract_ValidCommand_ShouldUpdateContract()
    {
        // Arrange
        var project = await CreateTestProjectAsync();

        var createCommand = new CreateContractCommand
        {
            ProjectId = project.Id,
            ContractNumber = $"CON-{Guid.NewGuid():N}".Substring(0, 20),
            ContractType = ContractType.FixedPrice,
            Title = "Original Title",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        var contractId = await _mediator.Send(createCommand);

        var updateCommand = new UpdateContractCommand
        {
            Id = contractId,
            Title = "Updated Title",
            Description = "Updated Description",
            ContractValueAmount = 75000m,
            ContractValueCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddMonths(12),
            Terms = "New payment terms"
        };

        // Act
        await _mediator.Send(updateCommand);

        // Assert
        var contract = await _contractRepository.GetByIdAsync(contractId);
        contract.Should().NotBeNull();
        contract!.Title.Should().Be("Updated Title");
        contract.Description.Should().Be("Updated Description");
        contract.ContractValue!.Amount.Should().Be(75000m);
        contract.Terms.Should().Be("New payment terms");
    }

    [Fact]
    public async Task DeleteContract_ValidCommand_ShouldDeleteContract()
    {
        // Arrange
        var project = await CreateTestProjectAsync();

        var createCommand = new CreateContractCommand
        {
            ProjectId = project.Id,
            ContractNumber = $"CON-{Guid.NewGuid():N}".Substring(0, 20),
            ContractType = ContractType.TimeAndMaterials,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        var contractId = await _mediator.Send(createCommand);

        var deleteCommand = new DeleteContractCommand { Id = contractId };

        // Act
        await _mediator.Send(deleteCommand);

        // Assert
        var contract = await _contractRepository.GetByIdAsync(contractId);
        contract.Should().BeNull();
    }

    private async Task<Project> CreateTestProjectAsync()
    {
        var client = await CreateTestClientAsync();

        var command = new CreateProjectCommand
        {
            ClientId = client.Id,
            Name = $"Test Project {Guid.NewGuid():N}",
            Description = "Test Description",
            ProjectType = ProjectType.Billable,
            BillingMode = BillingMode.TimeAndMaterials,
            StartDate = DateTime.UtcNow
        };

        var projectId = await _mediator.Send(command);
        return (await _projectRepository.GetByIdAsync(projectId!.Value))!;
    }

    private async Task<Client> CreateTestClientAsync()
    {
        var clientNumber = Client.GenerateClientNumber();
        var client = Client.Create(
            TestAuthenticationHelper.TestTenantId,
            clientNumber,
            $"Test Client {Guid.NewGuid():N}",
            ClientType.Corporate,
            primaryEmail: new Email("client@example.com")
        );

        DbContext.Set<Client>().Add(client);
        await DbContext.SaveChangesAsync();

        return client;
    }
}
