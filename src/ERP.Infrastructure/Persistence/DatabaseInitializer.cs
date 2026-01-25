using ERP.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Persistence;

/// <summary>
/// Extension methods for database initialization.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes the database (runs migrations and seeds data).
    /// </summary>
    public static async Task<IHost> InitializeDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ERPDbContext>>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            var context = services.GetRequiredService<ERPDbContext>();

            // Apply pending migrations
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Seed initial data
            logger.LogInformation("Seeding initial data...");
            var passwordHasher = services.GetRequiredService<IPasswordHasher>();
            var seeder = new DatabaseSeeder(context, passwordHasher, services.GetRequiredService<ILogger<DatabaseSeeder>>());
            await seeder.SeedAsync();
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }

        return host;
    }

    /// <summary>
    /// Checks if the database exists and can be connected to.
    /// </summary>
    public static async Task<bool> CanConnectAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ERPDbContext>();

        try
        {
            return await context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Core initialization logic shared by all entry points.
    /// </summary>
    public static async Task InitializeDatabaseCoreAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<ERPDbContext>>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            var context = services.GetRequiredService<ERPDbContext>();

            // Set a longer command timeout for migrations (5 minutes)
            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(15));

            // Apply pending migrations
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Seed initial data
            logger.LogInformation("Seeding initial data...");
            var passwordHasher = services.GetRequiredService<IPasswordHasher>();
            var seeder = new DatabaseSeeder(context, passwordHasher, services.GetRequiredService<ILogger<DatabaseSeeder>>());
            await seeder.SeedAsync();
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}
