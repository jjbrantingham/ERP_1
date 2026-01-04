using ERP.Application.Common.Exceptions;
using ERP.Application.PM.Commands;
using ERP.Domain.CRM.Entities;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
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
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = client.Id
        };

        // Act
        var projectId = await _mediator.Send(command);

        // Assert
        projectId.Should().BeGreaterThan(0);

        var project = await _projectRepository.GetByIdAsync(projectId);
        project.Should().NotBeNull();
        project!.Name.Should().Be("Test Project");
        project.Description.Should().Be("Test Description");
        project.Type.Should().Be(ProjectType.Billable);
        project.ClientId.Should().Be(client.Id);
        project.TenantId.Should().Be(TestAuthenticationHelper.TestTenantId);
        project.ProjectNumber.Should().NotBeNullOrEmpty();
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
            ClientId = 999999 // Non-existent client
        };

        // Act
        Func<Task> act = async () => await _mediator.Send(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Client*");
    }

    [Fact]
    public async Task Handle_MultipleProjects_ShouldHaveUniqueProjectNumbers()
    {
        // Arrange
        var client = await CreateTestClientAsync();

        var command1 = new CreateProjectCommand
        {
            Name = "Project 1",
            Description = "Description 1",
            Type = ProjectType.Billable,
            ClientId = client.Id
        };

        var command2 = new CreateProjectCommand
        {
            Name = "Project 2",
            Description = "Description 2",
            Type = ProjectType.Overhead,
            ClientId = client.Id
        };

        // Act
        var projectId1 = await _mediator.Send(command1);
        var projectId2 = await _mediator.Send(command2);

        // Assert
        var project1 = await _projectRepository.GetByIdAsync(projectId1);
        var project2 = await _projectRepository.GetByIdAsync(projectId2);

        project1!.ProjectNumber.Should().NotBe(project2!.ProjectNumber);
    }

    /// <summary>
    /// Helper method to create a test client.
    /// </summary>
    private async Task<Client> CreateTestClientAsync()
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

        DbContext.Set<Client>().Add(client);
        await DbContext.SaveChangesAsync();

        return client;
    }
}
