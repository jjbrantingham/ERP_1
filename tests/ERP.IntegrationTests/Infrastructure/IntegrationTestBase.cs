using ERP.Application.Common.Interfaces;
using ERP.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests providing common setup and utilities.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly IServiceScope Scope;
    protected readonly ERPDbContext DbContext;
    protected readonly IUnitOfWork UnitOfWork;

    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<ERPDbContext>();
        UnitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    /// <summary>
    /// Gets a service from the test service provider.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Clears all data from the database.
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        DbContext.RemoveRange(DbContext.ChangeTracker.Entries());
        await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        Scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}
