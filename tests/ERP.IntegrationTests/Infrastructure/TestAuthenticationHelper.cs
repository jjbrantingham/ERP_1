using ERP.Application.Common.Interfaces;
using Moq;

namespace ERP.IntegrationTests.Infrastructure;

/// <summary>
/// Helper for creating test authentication and tenant contexts.
/// </summary>
public static class TestAuthenticationHelper
{
    public static readonly Guid TestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly long TestUserId = 1L;
    public static readonly string TestUserName = "testuser@example.com";
    public static readonly string TestTenantName = "Test Tenant";

    /// <summary>
    /// Creates a mock ICurrentUserService for testing.
    /// </summary>
    public static ICurrentUserService CreateMockCurrentUser(
        long? userId = null,
        string? userName = null,
        bool isAuthenticated = true)
    {
        var mock = new Mock<ICurrentUserService>();

        mock.Setup(x => x.UserId).Returns(userId ?? TestUserId);
        mock.Setup(x => x.Username).Returns(userName ?? TestUserName);
        mock.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);

        return mock.Object;
    }

    /// <summary>
    /// Creates a mock ICurrentTenantService for testing.
    /// </summary>
    public static ICurrentTenantService CreateMockCurrentTenant(
        Guid? tenantId = null,
        string? tenantName = null)
    {
        var mock = new Mock<ICurrentTenantService>();

        mock.Setup(x => x.TenantId).Returns(tenantId ?? TestTenantId);
        mock.Setup(x => x.TenantName).Returns(tenantName ?? TestTenantName);

        return mock.Object;
    }

    /// <summary>
    /// Creates an unauthenticated user context for testing authorization failures.
    /// </summary>
    public static ICurrentUserService CreateUnauthenticatedUser()
    {
        return CreateMockCurrentUser(isAuthenticated: false);
    }
}
