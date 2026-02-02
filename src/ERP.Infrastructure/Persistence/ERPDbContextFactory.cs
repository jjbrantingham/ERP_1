using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ERP.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating ERPDbContext instances during EF Core migrations.
/// This is required because ERPDbContext has constructor dependencies that aren't available at design time.
/// </summary>
public class ERPDbContextFactory : IDesignTimeDbContextFactory<ERPDbContext>
{
    public ERPDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings.json in ERP.Web project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ERP.Web");

        // If running from solution root or different location, try to find ERP.Web
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();

            // Try to find ERP.Web by walking up the directory tree
            var current = new DirectoryInfo(basePath);
            while (current != null)
            {
                var webPath = Path.Combine(current.FullName, "src", "ERP.Web");
                if (Directory.Exists(webPath))
                {
                    basePath = webPath;
                    break;
                }
                current = current.Parent;
            }
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Fallback connection string for design-time if not found
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=ERP_Dev;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        var optionsBuilder = new DbContextOptionsBuilder<ERPDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(ERPDbContext).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });

        // Create design-time services that don't require HTTP context
        var tenantService = new DesignTimeTenantService();
        var userService = new DesignTimeUserService();
        var mediator = new DesignTimeMediator();

        return new ERPDbContext(optionsBuilder.Options, tenantService, userService, mediator);
    }
}

/// <summary>
/// Design-time tenant service that provides a default tenant for migrations.
/// </summary>
internal class DesignTimeTenantService : ERP.Application.Common.Interfaces.ICurrentTenantService
{
    // Use a fixed GUID for design-time operations
    private static readonly Guid DesignTimeTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid TenantId => DesignTimeTenantId;
    public string TenantName => "DesignTime";
    public bool IsSet => true;

    public void SetTenant(Guid tenantId, string? tenantName = null)
    {
        // No-op for design time
    }
}

/// <summary>
/// Design-time user service that provides a default user for migrations.
/// </summary>
internal class DesignTimeUserService : ERP.Application.Common.Interfaces.ICurrentUserService
{
    public long? UserId => null;
    public string? Username => "DesignTime";
    public Guid? TenantId => Guid.Parse("00000000-0000-0000-0000-000000000001");
    public bool IsAuthenticated => false;
    public IEnumerable<string> Roles => Enumerable.Empty<string>();
    public IEnumerable<string> Permissions => Enumerable.Empty<string>();
}

/// <summary>
/// Design-time mediator that does nothing (domain events aren't dispatched during migrations).
/// </summary>
internal class DesignTimeMediator : MediatR.IMediator
{
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(MediatR.IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return EmptyAsyncEnumerable<TResponse>();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        return EmptyAsyncEnumerable<object?>();
    }

    private static async IAsyncEnumerable<T> EmptyAsyncEnumerable<T>([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : MediatR.INotification
    {
        return Task.CompletedTask;
    }

    public Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : MediatR.IRequest
    {
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<object?>(null);
    }
}
