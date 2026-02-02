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
/// Integration tests for WBSItem command handlers.
/// </summary>
public class WBSItemCommandHandlerTests : IntegrationTestBase
{
    private readonly IMediator _mediator;
    private readonly IWBSItemRepository _wbsItemRepository;
    private readonly IProjectRepository _projectRepository;

    public WBSItemCommandHandlerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _mediator = GetService<IMediator>();
        _wbsItemRepository = GetService<IWBSItemRepository>();
        _projectRepository = GetService<IProjectRepository>();
    }

    [Fact]
    public async Task CreateWBSItem_ValidCommand_ShouldCreateWBSItem()
    {
        // Arrange
        var project = await CreateTestProjectAsync();

        var command = new CreateWBSItemCommand
        {
            ProjectId = project.Id,
            WBSCode = "1.0",
            Name = "Phase 1",
            Description = "First phase",
            SortOrder = 1,
            EstimatedHours = 160m,
            BudgetAmount = 25000m,
            BudgetCurrency = "USD"
        };

        // Act
        var wbsItemId = await _mediator.Send(command);

        // Assert
        wbsItemId.Should().BeGreaterThan(0);

        var wbsItem = await _wbsItemRepository.GetByIdAsync(wbsItemId);
        wbsItem.Should().NotBeNull();
        wbsItem!.ProjectId.Should().Be(project.Id);
        wbsItem.Code.Should().Be("1.0");
        wbsItem.Name.Should().Be("Phase 1");
        wbsItem.EstimatedHours.Should().Be(160m);
        wbsItem.Budget!.Amount.Should().Be(25000m);
        wbsItem.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateWBSItem_WithParent_ShouldSetParentId()
    {
        // Arrange
        var project = await CreateTestProjectAsync();

        var parentCommand = new CreateWBSItemCommand
        {
            ProjectId = project.Id,
            WBSCode = "1.0",
            Name = "Phase 1",
            SortOrder = 1
        };

        var parentId = await _mediator.Send(parentCommand);

        var childCommand = new CreateWBSItemCommand
        {
            ProjectId = project.Id,
            ParentId = parentId,
            WBSCode = "1.1",
            Name = "Sub-Phase 1.1",
            SortOrder = 1
        };

        // Act
        var childId = await _mediator.Send(childCommand);

        // Assert
        var childItem = await _wbsItemRepository.GetByIdAsync(childId);
        childItem.Should().NotBeNull();
        childItem!.ParentId.Should().Be(parentId);
        childItem.Level.Should().Be(2); // 1.1 has level 2
    }

    [Fact]
    public async Task UpdateWBSItem_ValidCommand_ShouldUpdateWBSItem()
    {
        // Arrange
        var project = await CreateTestProjectAsync();

        var createCommand = new CreateWBSItemCommand
        {
            ProjectId = project.Id,
            WBSCode = "1.0",
            Name = "Original Name",
            SortOrder = 1
        };

        var wbsItemId = await _mediator.Send(createCommand);

        var updateCommand = new UpdateWBSItemCommand
        {
            Id = wbsItemId,
            Name = "Updated Name",
            Description = "Updated Description",
            SortOrder = 2,
            EstimatedHours = 200m,
            BudgetAmount = 50000m,
            BudgetCurrency = "USD"
        };

        // Act
        await _mediator.Send(updateCommand);

        // Assert
        var wbsItem = await _wbsItemRepository.GetByIdAsync(wbsItemId);
        wbsItem.Should().NotBeNull();
        wbsItem!.Name.Should().Be("Updated Name");
        wbsItem.Description.Should().Be("Updated Description");
        wbsItem.DisplayOrder.Should().Be(2);
        wbsItem.EstimatedHours.Should().Be(200m);
        wbsItem.Budget!.Amount.Should().Be(50000m);
    }

    [Fact]
    public async Task DeleteWBSItem_ValidCommand_ShouldDeleteWBSItem()
    {
        // Arrange
        var project = await CreateTestProjectAsync();

        var createCommand = new CreateWBSItemCommand
        {
            ProjectId = project.Id,
            WBSCode = "1.0",
            Name = "Phase to Delete",
            SortOrder = 1
        };

        var wbsItemId = await _mediator.Send(createCommand);

        var deleteCommand = new DeleteWBSItemCommand { Id = wbsItemId };

        // Act
        await _mediator.Send(deleteCommand);

        // Assert
        var wbsItem = await _wbsItemRepository.GetByIdAsync(wbsItemId);
        wbsItem.Should().BeNull();
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
