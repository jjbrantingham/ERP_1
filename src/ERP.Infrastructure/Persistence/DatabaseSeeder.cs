using ERP.Domain.Common.Entities;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.HR.Entities;
using ERP.Domain.Identity.Entities;
using ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data into the database.
/// </summary>
public class DatabaseSeeder
{
    private readonly ERPDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ERPDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all initial data.
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        try
        {
            // Seed in order of dependencies
            var defaultTenant = await SeedDefaultTenantAsync();
            await SeedRolesAsync(defaultTenant.TenantId);
            await SeedSystemAdministratorAsync(defaultTenant.TenantId);
            await SeedResourceTypesAsync(defaultTenant.TenantId);

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task<Tenant> SeedDefaultTenantAsync()
    {
        if (await _context.Tenants.AnyAsync())
        {
            _logger.LogInformation("Tenants already exist, skipping tenant seed");
            return await _context.Tenants.FirstAsync();
        }

        _logger.LogInformation("Seeding default tenant...");

        var tenant = Tenant.Create(
            companyName: "Default Company",
            subdomain: "default",
            defaultCurrency: "USD",
            maxUsers: 100,
            subscriptionPlan: "Enterprise",
            timeZone: "UTC"
        );

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Default tenant created with ID: {TenantId}", tenant.TenantGuid);

        return tenant;
    }

    private async Task SeedRolesAsync(Guid tenantId)
    {
        if (await _context.Roles.AnyAsync())
        {
            _logger.LogInformation("Roles already exist, skipping role seed");
            return;
        }

        _logger.LogInformation("Seeding roles and permissions...");

        var roles = new List<(string Name, string Description, bool IsSystemRole, List<string> Permissions)>
        {
            (
                Roles.SystemAdministrator,
                "Full system access with all permissions",
                true,
                Permissions.GetAllPermissions().ToList()
            ),
            (
                Roles.TenantAdministrator,
                "Full access within tenant",
                true,
                Permissions.GetAllPermissions()
                    .Where(p => !p.StartsWith("System.") && !p.StartsWith("Tenant."))
                    .ToList()
            ),
            (
                Roles.ProjectManager,
                "Manages projects, resources, and schedules",
                false,
                new List<string>
                {
                    Permissions.ProjectsView, Permissions.ProjectsCreate, Permissions.ProjectsEdit,
                    Permissions.ProjectsManageWBS, Permissions.ProjectsManageResources,
                    Permissions.ClientsView,
                    Permissions.EmployeesView,
                    Permissions.TimesheetsView, Permissions.TimesheetsApprove,
                    Permissions.ExpensesView, Permissions.ExpensesApprove,
                    Permissions.ReportsView, Permissions.DashboardsView
                }
            ),
            (
                Roles.FinanceManager,
                "Manages financial operations and billing",
                false,
                new List<string>
                {
                    Permissions.AccountsView, Permissions.AccountsCreate, Permissions.AccountsEdit,
                    Permissions.JournalEntriesView, Permissions.JournalEntriesCreate, Permissions.JournalEntriesPost,
                    Permissions.InvoicesView, Permissions.InvoicesCreate, Permissions.InvoicesEdit, Permissions.InvoicesSend,
                    Permissions.PaymentsRecord,
                    Permissions.ReportsView, Permissions.ReportsExport,
                    Permissions.DashboardsView
                }
            ),
            (
                Roles.HRManager,
                "Manages employees and HR operations",
                false,
                new List<string>
                {
                    Permissions.EmployeesView, Permissions.EmployeesCreate, Permissions.EmployeesEdit,
                    Permissions.EmployeesManageRates,
                    Permissions.UsersView, Permissions.UsersCreate, Permissions.UsersEdit,
                    Permissions.ReportsView, Permissions.DashboardsView
                }
            ),
            (
                Roles.Accountant,
                "Manages accounting and financial reports",
                false,
                new List<string>
                {
                    Permissions.AccountsView, Permissions.JournalEntriesView, Permissions.JournalEntriesCreate,
                    Permissions.InvoicesView, Permissions.PaymentsRecord,
                    Permissions.ReportsView, Permissions.ReportsExport,
                    Permissions.DashboardsView
                }
            ),
            (
                Roles.Employee,
                "Standard employee with time and expense entry",
                false,
                new List<string>
                {
                    Permissions.TimesheetsView, Permissions.TimesheetsCreate, Permissions.TimesheetsEdit,
                    Permissions.TimesheetsSubmit,
                    Permissions.ExpensesView, Permissions.ExpensesCreate, Permissions.ExpensesEdit,
                    Permissions.ExpensesSubmit,
                    Permissions.ProjectsView,
                    Permissions.DashboardsView
                }
            ),
            (
                Roles.Client,
                "External user with limited access",
                false,
                new List<string>
                {
                    Permissions.ProjectsView,
                    Permissions.InvoicesView,
                    Permissions.ReportsView,
                    Permissions.DashboardsView
                }
            ),
            (
                Roles.Viewer,
                "Read-only access to reports and dashboards",
                false,
                new List<string>
                {
                    Permissions.ReportsView,
                    Permissions.DashboardsView
                }
            )
        };

        foreach (var (name, description, isSystemRole, permissions) in roles)
        {
            var role = Role.Create(tenantId, name, description, isSystemRole);

            foreach (var permission in permissions)
            {
                role.AddPermission(permission);
            }

            _context.Roles.Add(role);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} roles with permissions", roles.Count);
    }

    private async Task SeedSystemAdministratorAsync(Guid tenantId)
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist, skipping admin user seed");
            return;
        }

        _logger.LogInformation("Seeding system administrator...");

        // Create admin user
        // Password: Admin@123 (CHANGE THIS IN PRODUCTION!)
        var passwordHash = "lYtHr8F7qJ9K2mN3pQ4sV6wX8zA1bC5dE7fG9hI0jK2lM4nO6pR8sT0uW2xY4zA6bC8dF0gH2jK4mN6pQ8sV0wX2zA4b"; // This is a placeholder

        var adminUser = User.Create(
            tenantId,
            "admin",
            new Email("admin@erp.local"),
            passwordHash,
            "System",
            "Administrator",
            null
        );

        // Get System Administrator role
        var adminRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == Roles.SystemAdministrator);

        if (adminRole != null)
        {
            adminUser.AddRole(adminRole.Id);
        }

        adminUser.ConfirmEmail(); // Auto-confirm admin email

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("System administrator created: admin@erp.local");
        _logger.LogWarning("DEFAULT PASSWORD IS SET! Please change it immediately after first login!");
    }

    private async Task SeedResourceTypesAsync(Guid tenantId)
    {
        if (await _context.ResourceTypes.AnyAsync())
        {
            _logger.LogInformation("Resource types already exist, skipping resource type seed");
            return;
        }

        _logger.LogInformation("Seeding resource types...");

        var resourceTypes = new List<(string Name, string Description, string Code, int DisplayOrder)>
        {
            ("Software Developer", "Software development and programming", "DEV", 1),
            ("Senior Software Developer", "Senior software development and architecture", "SDEV", 2),
            ("QA Engineer", "Quality assurance and testing", "QA", 3),
            ("DevOps Engineer", "DevOps and infrastructure", "DEVOPS", 4),
            ("Project Manager", "Project management and coordination", "PM", 5),
            ("Business Analyst", "Business analysis and requirements", "BA", 6),
            ("UI/UX Designer", "User interface and experience design", "DESIGN", 7),
            ("Technical Writer", "Documentation and technical writing", "WRITER", 8),
            ("Database Administrator", "Database management and administration", "DBA", 9),
            ("System Administrator", "System administration and support", "SYSADMIN", 10)
        };

        foreach (var (name, description, code, displayOrder) in resourceTypes)
        {
            var resourceType = ResourceType.Create(
                tenantId,
                name,
                description,
                code,
                displayOrder
            );

            _context.ResourceTypes.Add(resourceType);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} resource types", resourceTypes.Count);
    }
}
