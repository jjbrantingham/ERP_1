using ERP.Infrastructure.Persistence;

namespace ERP.Web.Extensions;

/// <summary>
/// Extension methods for WebApplication.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Initializes the database (runs migrations and seeds data).
    /// </summary>
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await DatabaseInitializer.InitializeDatabaseCoreAsync(scope.ServiceProvider);
        return app;
    }
}
