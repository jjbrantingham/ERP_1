using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ERP.Infrastructure.HealthChecks;

/// <summary>
/// Health check that tracks application startup status.
/// Useful for Kubernetes readiness probes.
/// </summary>
public class StartupHealthCheck : IHealthCheck
{
    private volatile bool _isReady;

    /// <summary>
    /// Marks the application as ready to accept traffic
    /// </summary>
    public void MarkAsReady()
    {
        _isReady = true;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_isReady)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy("Application is ready"));
        }

        return Task.FromResult(
            HealthCheckResult.Unhealthy("Application is still starting up"));
    }
}
