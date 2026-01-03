using ERP.Application.AUDIT.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically applies data retention policies to audit logs.
/// Runs daily at 2:00 AM UTC by default.
/// </summary>
public class DataRetentionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataRetentionBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour
    private readonly TimeSpan _targetRunTime = new TimeSpan(2, 0, 0); // 2:00 AM

    public DataRetentionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DataRetentionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Retention Background Service started");

        var lastRunDate = DateTime.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                // Check if it's time to run (daily at target time)
                if (now.TimeOfDay >= _targetRunTime &&
                    lastRunDate.Date < today)
                {
                    _logger.LogInformation("Starting data retention policy execution");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var retentionService = scope.ServiceProvider
                            .GetRequiredService<IDataRetentionPolicyService>();

                        var purgedCount = await retentionService.ApplyRetentionPoliciesAsync(stoppingToken);

                        _logger.LogInformation(
                            "Data retention policy execution completed. Purged {Count} audit logs",
                            purgedCount);
                    }

                    lastRunDate = today;
                }

                // Wait before next check
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error executing data retention policy");

                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("Data Retention Background Service stopped");
    }
}
