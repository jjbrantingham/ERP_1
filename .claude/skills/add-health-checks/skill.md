# Add Health Checks Skill

## Purpose
Add health check endpoints for monitoring application health.

## Configure Health Checks

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ERPDbContext>("database")
    .AddAzureBlobStorage(connectionString, "blob-storage")
    .AddCheck<CustomHealthCheck>("custom-check");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");
```

## Custom Health Check

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ERPDbContext _context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
```

## Related Skills
- add-logging
