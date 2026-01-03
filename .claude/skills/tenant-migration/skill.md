# Tenant Migration Skill

## Purpose
Migrate tenant data safely with proper isolation and validation.

## Migration Process

```csharp
public class TenantMigrationService
{
    public async Task MigrateTenantAsync(Guid sourceTenantId, Guid targetTenantId)
    {
        _logger.LogInformation("Starting tenant migration from {Source} to {Target}",
            sourceTenantId, targetTenantId);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validate tenants
            await ValidateTenantsAsync(sourceTenantId, targetTenantId);

            // 2. Migrate data
            await MigrateProjectsAsync(sourceTenantId, targetTenantId);
            await MigrateEmployeesAsync(sourceTenantId, targetTenantId);
            await MigrateTimesheetsAsync(sourceTenantId, targetTenantId);

            // 3. Verify data integrity
            await VerifyMigrationAsync(sourceTenantId, targetTenantId);

            await transaction.CommitAsync();

            _logger.LogInformation("Tenant migration completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant migration failed");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

## Related Skills
- verify-tenant-isolation
