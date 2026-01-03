using Microsoft.Extensions.Diagnostics.HealthChecks;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.HealthChecks;

/// <summary>
/// Custom health check for database connectivity and responsiveness
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ERPDbContext _dbContext;

    public DatabaseHealthCheck(ERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connectivity with a simple query
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to database",
                    data: new Dictionary<string, object>
                    {
                        ["Database"] = _dbContext.Database.GetConnectionString() ?? "Unknown"
                    });
            }

            // Execute a simple query to verify database is responsive
            var startTime = DateTime.UtcNow;
            var count = await _dbContext.Users.CountAsync(cancellationToken);
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            var data = new Dictionary<string, object>
            {
                ["Database"] = _dbContext.Database.GetConnectionString() ?? "Unknown",
                ["ResponseTimeMs"] = responseTime,
                ["UserCount"] = count
            };

            // Warn if response time is slow (> 1 second)
            if (responseTime > 1000)
            {
                return HealthCheckResult.Degraded(
                    $"Database is responding slowly ({responseTime:F2}ms)",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Database is healthy (response time: {responseTime:F2}ms)",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["Database"] = _dbContext.Database.GetConnectionString() ?? "Unknown",
                    ["Error"] = ex.Message
                });
        }
    }
}
