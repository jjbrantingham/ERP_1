using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ERP.Infrastructure.HealthChecks;

/// <summary>
/// Options for configuring memory health check thresholds
/// </summary>
public class MemoryHealthCheckOptions
{
    /// <summary>
    /// Memory threshold in bytes for degraded status (default: 1GB)
    /// </summary>
    public long DegradedThreshold { get; set; } = 1024L * 1024L * 1024L; // 1GB

    /// <summary>
    /// Memory threshold in bytes for unhealthy status (default: 2GB)
    /// </summary>
    public long UnhealthyThreshold { get; set; } = 2L * 1024L * 1024L * 1024L; // 2GB
}

/// <summary>
/// Health check that monitors application memory usage
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly MemoryHealthCheckOptions _options;

    public MemoryHealthCheck(IOptions<MemoryHealthCheckOptions> options)
    {
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var gcInfo = GC.GetGCMemoryInfo();

        var data = new Dictionary<string, object>
        {
            ["AllocatedBytes"] = allocated,
            ["AllocatedMB"] = allocated / 1024 / 1024,
            ["Gen0Collections"] = GC.CollectionCount(0),
            ["Gen1Collections"] = GC.CollectionCount(1),
            ["Gen2Collections"] = GC.CollectionCount(2),
            ["HeapSizeBytes"] = gcInfo.HeapSizeBytes,
            ["MemoryLoadBytes"] = gcInfo.MemoryLoadBytes
        };

        if (allocated >= _options.UnhealthyThreshold)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Memory usage is critical: {allocated / 1024 / 1024}MB",
                data: data));
        }

        if (allocated >= _options.DegradedThreshold)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Memory usage is high: {allocated / 1024 / 1024}MB",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Memory usage is normal: {allocated / 1024 / 1024}MB",
            data: data));
    }
}
