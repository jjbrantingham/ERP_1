using ERP.Application.PM.Commands;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using ERP.Domain.PM.ValueObjects;
using ERP.IntegrationTests.Infrastructure;
using FluentAssertions;
using MediatR;

namespace ERP.IntegrationTests.PM;

/// <summary>
/// Integration tests for CreateProjectCommandHandler.
/// Tests command handling, database persistence, and business logic.
/// </summary>
public class CreateProjectCommandHandlerTests : IntegrationTestBase
{
    private readonly IMediator _mediator;
    private readonly IProjectRepository _projectRepository;

    public CreateProjectCommandHandlerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _mediator = GetService<IMediator>();
        _projectRepository = GetService<IProjectRepository>();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProject()
    {
        // Arrange
        var client = await CreateTestClientAsync();

        var command = new CreateProjectCommand
        {
            ClientId = client.Id,
            Name = "Test Project",
            Description = "Test Description",
            ProjectType = ProjectType.Billable,
            BillingMode = BillingMode.TimeAndMaterials,
            StartDate = DateTime.UtcNow
        };

        // Act
        var projectId = await _mediator.Send(command);

        // Assert
        projectId.Should().NotBeNull();
        ((long)projectId!).Should().BeGreaterThan(0);

        var project = await _projectRepository.GetByIdAsync((long)projectId);
        project.Should().NotBeNull();
        project!.Name.Should().Be("Test Project");
        project.Description.Should().Be("Test Description");
        project.ProjectType.Should().Be(ProjectType.Billable);
        project.ClientId.Should().Be(client.Id);
        project.TenantId.Should().Be(TestAuthenticationHelper.TestTenantId);
        project.ProjectNumber.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_MultipleProjects_ShouldHaveUniqueProjectNumbers()
    {
        // Arrange
        var client = await CreateTestClientAsync();

        var command1 = new CreateProjectCommand
        {
            ClientId = client.Id,
            Name = "Project 1",
            Description = "Description 1",
            ProjectType = ProjectType.Billable,
            BillingMode = BillingMode.TimeAndMaterials,
            StartDate = DateTime.UtcNow
        };

        var command2 = new CreateProjectCommand
        {
            ClientId = client.Id,
            Name = "Project 2",
            Description = "Description 2",
            ProjectType = ProjectType.Internal,
            BillingMode = BillingMode.NonBillable,
            StartDate = DateTime.UtcNow
        };

        // Act
        var projectId1 = await _mediator.Send(command1);
        var projectId2 = await _mediator.Send(command2);

        // Assert
        projectId1.Should().NotBeNull();
        projectId2.Should().NotBeNull();
        var project1 = await _projectRepository.GetByIdAsync((long)projectId1!);
        var project2 = await _projectRepository.GetByIdAsync((long)projectId2!);

        project1!.ProjectNumber.Should().NotBe(project2!.ProjectNumber);
    }

    /// <summary>
    /// Helper method to create a test client.
    /// </summary>
    private async Task<Client> CreateTestClientAsync()
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

        return client;
    }
}
