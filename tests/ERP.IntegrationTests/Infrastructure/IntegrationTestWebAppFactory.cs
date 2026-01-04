using ERP.Application.Common.Interfaces;
using ERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ERP.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures in-memory database and test-specific services.
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ERPDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using in-memory database for testing
            services.AddDbContext<ERPDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
                options.EnableSensitiveDataLogging();
            });

            // Override authentication services with test mocks
            services.RemoveAll<ICurrentUserService>();
            services.RemoveAll<ICurrentTenantService>();

            services.AddScoped<ICurrentUserService>(sp =>
                TestAuthenticationHelper.CreateMockCurrentUser());

            services.AddScoped<ICurrentTenantService>(sp =>
                TestAuthenticationHelper.CreateMockCurrentTenant());

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ERPDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            }
        });

        builder.UseEnvironment("Testing");
    }
}
