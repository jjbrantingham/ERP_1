using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.PM.Commands;
using ERP.Domain.CRM.Entities;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Enums;
using ERP.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ERP.IntegrationTests.Security;

/// <summary>
/// Integration tests for authorization checks across command handlers.
/// Verifies that unauthenticated users are properly rejected.
/// </summary>
public class AuthorizationTests : IntegrationTestBase
{
    public AuthorizationTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateProject_UnauthenticatedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange - Create test client with default authenticated user
        var client = await CreateTestClientAsync();

        // Create a custom factory with unauthenticated user
        var customFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override ICurrentUserService with unauthenticated user
                services.AddScoped<ICurrentUserService>(sp =>
                    TestAuthenticationHelper.CreateUnauthenticatedUser());
            });
        });

        // Create new scope with unauthenticated context
        using var scope = customFactory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = client.Id
        };

        // Act & Assert
        // The handler will call AuthorizationHelper.EnsureAuthenticated()
        // which should throw UnauthorizedException for unauthenticated users
        await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await mediator.Send(command));
    }

    [Fact]
    public async Task CreateProject_NullUserId_ShouldThrowUnauthorizedException()
    {
        // Arrange - Create test client with default authenticated user
        var client = await CreateTestClientAsync();

        // Create a custom factory with null user ID
        var customFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override ICurrentUserService with null UserId
                services.AddScoped<ICurrentUserService>(sp =>
                    TestAuthenticationHelper.CreateMockCurrentUser(
                        userId: null,
                        isAuthenticated: true)); // Authenticated but no UserId
            });
        });

        // Create new scope with null UserId context
        using var scope = customFactory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = client.Id
        };

        // Act & Assert
        // The AuthorizationHelper.EnsureAuthenticated() method should reject:
        // - IsAuthenticated = false (tested in previous test)
        // - UserId = null (tested here)
        // - Missing claims
        await Assert.ThrowsAsync<UnauthorizedException>(async () =>
            await mediator.Send(command));
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
