using ERP.Application.PM.Commands;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.HR.Entities;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using ERP.IntegrationTests.Infrastructure;
using FluentAssertions;
using MediatR;

namespace ERP.IntegrationTests.PM;

/// <summary>
/// Integration tests for ResourceAllocation command handlers.
/// </summary>
public class ResourceAllocationCommandHandlerTests : IntegrationTestBase
{
    private readonly IMediator _mediator;
    private readonly IResourceAllocationRepository _resourceAllocationRepository;
    private readonly IProjectRepository _projectRepository;

    public ResourceAllocationCommandHandlerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _mediator = GetService<IMediator>();
        _resourceAllocationRepository = GetService<IResourceAllocationRepository>();
        _projectRepository = GetService<IProjectRepository>();
    }

    [Fact]
    public async Task CreateResourceAllocation_ValidCommand_ShouldCreateAllocation()
    {
        // Arrange
        var project = await CreateTestProjectAsync();
        var employee = await CreateTestEmployeeAsync();

        var command = new CreateResourceAllocationCommand
        {
            ProjectId = project.Id,
            EmployeeId = employee.Id,
            StartDate = DateTime.UtcNow,
            AllocatedHoursPerWeek = 40m,
            Role = "Developer"
        };

        // Act
        var allocationId = await _mediator.Send(command);

        // Assert
        allocationId.Should().BeGreaterThan(0);

        var allocation = await _resourceAllocationRepository.GetByIdAsync(allocationId);
        allocation.Should().NotBeNull();
        allocation!.ProjectId.Should().Be(project.Id);
        allocation.EmployeeId.Should().Be(employee.Id);
        allocation.AllocatedHoursPerWeek.Should().Be(40m);
        allocation.Role.Should().Be("Developer");
        allocation.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateResourceAllocation_DuplicateAllocation_ShouldThrowException()
    {
        // Arrange
        var project = await CreateTestProjectAsync();
        var employee = await CreateTestEmployeeAsync();

        var command = new CreateResourceAllocationCommand
        {
            ProjectId = project.Id,
            EmployeeId = employee.Id,
            StartDate = DateTime.UtcNow,
            AllocatedHoursPerWeek = 40m
        };

        // First allocation
        await _mediator.Send(command);

        // Act & Assert - Second allocation should fail
        await Assert.ThrowsAsync<InvalidOperationException>(() => _mediator.Send(command));
    }

    [Fact]
    public async Task UpdateResourceAllocation_ValidCommand_ShouldUpdateAllocation()
    {
        // Arrange
        var project = await CreateTestProjectAsync();
        var employee = await CreateTestEmployeeAsync();

        var createCommand = new CreateResourceAllocationCommand
        {
            ProjectId = project.Id,
            EmployeeId = employee.Id,
            StartDate = DateTime.UtcNow,
            AllocatedHoursPerWeek = 40m,
            Role = "Developer"
        };

        var allocationId = await _mediator.Send(createCommand);

        var updateCommand = new UpdateResourceAllocationCommand
        {
            Id = allocationId,
            AllocatedHoursPerWeek = 30m,
            Role = "Senior Developer"
        };

        // Act
        await _mediator.Send(updateCommand);

        // Assert
        var allocation = await _resourceAllocationRepository.GetByIdAsync(allocationId);
        allocation.Should().NotBeNull();
        allocation!.AllocatedHoursPerWeek.Should().Be(30m);
        allocation.Role.Should().Be("Senior Developer");
    }

    [Fact]
    public async Task DeleteResourceAllocation_ValidCommand_ShouldDeleteAllocation()
    {
        // Arrange
        var project = await CreateTestProjectAsync();
        var employee = await CreateTestEmployeeAsync();

        var createCommand = new CreateResourceAllocationCommand
        {
            ProjectId = project.Id,
            EmployeeId = employee.Id,
            StartDate = DateTime.UtcNow,
            AllocatedHoursPerWeek = 40m
        };

        var allocationId = await _mediator.Send(createCommand);

        var deleteCommand = new DeleteResourceAllocationCommand { Id = allocationId };

        // Act
        await _mediator.Send(deleteCommand);

        // Assert
        var allocation = await _resourceAllocationRepository.GetByIdAsync(allocationId);
        allocation.Should().BeNull();
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

    private async Task<Employee> CreateTestEmployeeAsync()
    {
        var employeeNumber = Employee.GenerateEmployeeNumber();
        var employee = Employee.Create(
            TestAuthenticationHelper.TestTenantId,
            employeeNumber,
            "Test",
            "Employee",
            new Email($"test{Guid.NewGuid():N}@example.com")
        );

        DbContext.Set<Employee>().Add(employee);
        await DbContext.SaveChangesAsync();

        return employee;
    }
}
