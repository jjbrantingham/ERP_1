using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Entities;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.ValueObjects;
using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Enums;
using ERP.Domain.HR.ValueObjects;
using ERP.Domain.Identity.Entities;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.ValueObjects;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    // Default admin credentials for local development
    // Username: admin
    // Password: Admin@123!
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminEmail = "admin@erp.local";
    public const string DefaultAdminPassword = "Admin@123!";

    public DatabaseSeeder(ERPDbContext context, IPasswordHasher passwordHasher, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
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

            // Seed test data for local development
            await SeedChartOfAccountsAsync(defaultTenant.TenantId);
            await SeedClientsAsync(defaultTenant.TenantId);
            await SeedEmployeesAsync(defaultTenant.TenantId);
            await SeedProjectsAsync(defaultTenant.TenantId);

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
        // Use IgnoreQueryFilters to bypass tenant filtering during seeding
        if (await _context.Tenants.IgnoreQueryFilters().AnyAsync())
        {
            _logger.LogInformation("Tenants already exist, skipping tenant seed");
            return await _context.Tenants.IgnoreQueryFilters().FirstAsync();
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
        // Use IgnoreQueryFilters to bypass tenant filtering during seeding
        if (await _context.Roles.IgnoreQueryFilters().AnyAsync())
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
        // Use IgnoreQueryFilters to bypass tenant filtering during seeding
        if (await _context.Users.IgnoreQueryFilters().AnyAsync())
        {
            _logger.LogInformation("Users already exist, skipping admin user seed");
            return;
        }

        _logger.LogInformation("Seeding system administrator...");

        // Create admin user with properly hashed password
        var passwordHash = _passwordHasher.HashPassword(DefaultAdminPassword);

        var adminUser = User.Create(
            tenantId,
            DefaultAdminUsername,
            new Email(DefaultAdminEmail),
            passwordHash,
            "System",
            "Administrator",
            null
        );

        // Get System Administrator role (bypass tenant filter during seeding)
        var adminRole = await _context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == Roles.SystemAdministrator);

        if (adminRole != null)
        {
            adminUser.AddRole(adminRole.Id);
        }

        adminUser.ConfirmEmail(); // Auto-confirm admin email

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("===========================================");
        _logger.LogInformation("System administrator created successfully!");
        _logger.LogInformation("Username: {Username}", DefaultAdminUsername);
        _logger.LogInformation("Email: {Email}", DefaultAdminEmail);
        _logger.LogInformation("Password: {Password}", DefaultAdminPassword);
        _logger.LogInformation("===========================================");
        _logger.LogWarning("IMPORTANT: Change the default password in production!");
    }

    private async Task SeedResourceTypesAsync(Guid tenantId)
    {
        // Use IgnoreQueryFilters to bypass tenant filtering during seeding
        if (await _context.ResourceTypes.IgnoreQueryFilters().AnyAsync())
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

    private async Task SeedChartOfAccountsAsync(Guid tenantId)
    {
        // Use IgnoreQueryFilters to bypass tenant filtering during seeding
        if (await _context.Accounts.IgnoreQueryFilters().AnyAsync())
        {
            _logger.LogInformation("Accounts already exist, skipping chart of accounts seed");
            return;
        }

        _logger.LogInformation("Seeding chart of accounts...");

        var accounts = new List<(string Number, string Name, AccountType Type, string? Description)>
        {
            // Assets (1000-1999)
            ("1000", "Cash", AccountType.Asset, "Cash on hand and in bank accounts"),
            ("1010", "Checking Account", AccountType.Asset, "Primary business checking account"),
            ("1020", "Savings Account", AccountType.Asset, "Business savings account"),
            ("1100", "Accounts Receivable", AccountType.Asset, "Money owed by customers"),
            ("1200", "Prepaid Expenses", AccountType.Asset, "Expenses paid in advance"),
            ("1500", "Equipment", AccountType.Asset, "Office and computer equipment"),
            ("1510", "Accumulated Depreciation - Equipment", AccountType.Asset, "Accumulated depreciation on equipment"),

            // Liabilities (2000-2999)
            ("2000", "Accounts Payable", AccountType.Liability, "Money owed to vendors"),
            ("2100", "Accrued Expenses", AccountType.Liability, "Expenses incurred but not yet paid"),
            ("2200", "Deferred Revenue", AccountType.Liability, "Revenue received but not yet earned"),
            ("2300", "Payroll Liabilities", AccountType.Liability, "Payroll taxes and withholdings owed"),
            ("2500", "Notes Payable", AccountType.Liability, "Long-term loans and notes"),

            // Equity (3000-3999)
            ("3000", "Owner's Equity", AccountType.Equity, "Owner's investment in the business"),
            ("3100", "Retained Earnings", AccountType.Equity, "Accumulated profits retained in business"),
            ("3200", "Current Year Earnings", AccountType.Equity, "Net income for current year"),

            // Revenue (4000-4999)
            ("4000", "Service Revenue", AccountType.Revenue, "Revenue from professional services"),
            ("4010", "Consulting Revenue", AccountType.Revenue, "Revenue from consulting services"),
            ("4020", "Development Revenue", AccountType.Revenue, "Revenue from software development"),
            ("4100", "Project Revenue", AccountType.Revenue, "Revenue from project work"),
            ("4500", "Other Income", AccountType.Revenue, "Miscellaneous income"),

            // Expenses (5000-9999)
            ("5000", "Cost of Services", AccountType.Expense, "Direct costs of providing services"),
            ("5100", "Salaries and Wages", AccountType.Expense, "Employee compensation"),
            ("5110", "Contractor Expenses", AccountType.Expense, "Payments to contractors"),
            ("5200", "Payroll Taxes", AccountType.Expense, "Employer payroll tax expenses"),
            ("5300", "Employee Benefits", AccountType.Expense, "Health insurance and other benefits"),

            ("6000", "Rent Expense", AccountType.Expense, "Office rent and lease payments"),
            ("6100", "Utilities", AccountType.Expense, "Electric, gas, water, internet"),
            ("6200", "Office Supplies", AccountType.Expense, "Office supplies and materials"),
            ("6300", "Software Subscriptions", AccountType.Expense, "Software and SaaS subscriptions"),
            ("6400", "Professional Fees", AccountType.Expense, "Legal, accounting, consulting fees"),
            ("6500", "Insurance", AccountType.Expense, "Business insurance premiums"),
            ("6600", "Travel Expense", AccountType.Expense, "Business travel costs"),
            ("6700", "Marketing and Advertising", AccountType.Expense, "Marketing and promotional expenses"),
            ("6800", "Depreciation Expense", AccountType.Expense, "Depreciation of assets"),
            ("6900", "Miscellaneous Expense", AccountType.Expense, "Other business expenses")
        };

        foreach (var (number, name, type, description) in accounts)
        {
            var account = Account.Create(
                tenantId,
                new AccountNumber(number),
                name,
                type,
                "USD",
                allowPosting: true,
                description: description
            );

            _context.Accounts.Add(account);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} accounts", accounts.Count);
    }

    private async Task SeedClientsAsync(Guid tenantId)
    {
        // Use IgnoreQueryFilters to bypass tenant filtering during seeding
        if (await _context.Clients.IgnoreQueryFilters().AnyAsync())
        {
            _logger.LogInformation("Clients already exist, skipping client seed");
            return;
        }

        _logger.LogInformation("Seeding test clients...");

        var clients = new List<(string Number, string Name, ClientType Type, string Email, string? Industry, string? Phone)>
        {
            ("CLI-001", "Acme Corporation", ClientType.Corporate, "contact@acme.com", "Technology", "555-100-1000"),
            ("CLI-002", "TechStart Inc.", ClientType.Corporate, "info@techstart.com", "Software", "555-100-2000"),
            ("CLI-003", "Global Services Ltd", ClientType.Corporate, "hello@globalservices.com", "Consulting", "555-100-3000"),
            ("CLI-004", "Smith & Associates", ClientType.Corporate, "office@smithassoc.com", "Legal", "555-100-4000"),
            ("CLI-005", "Downtown Retail", ClientType.Corporate, "manager@downtownretail.com", "Retail", "555-100-5000"),
            ("CLI-006", "HealthCare Plus", ClientType.Corporate, "admin@healthcareplus.com", "Healthcare", "555-100-6000"),
            ("CLI-007", "EduTech Solutions", ClientType.Corporate, "support@edutech.com", "Education", "555-100-7000"),
            ("CLI-008", "Green Energy Co", ClientType.Corporate, "info@greenenergy.com", "Energy", "555-100-8000"),
            ("CLI-009", "John Smith Consulting", ClientType.Individual, "john.smith@email.com", "Consulting", "555-200-1000"),
            ("CLI-010", "Jane Doe Freelance", ClientType.Individual, "jane.doe@email.com", "Marketing", "555-200-2000")
        };

        foreach (var (number, name, type, email, industry, phone) in clients)
        {
            var client = Client.Create(
                tenantId,
                number,
                name,
                type,
                primaryEmail: new Email(email),
                industry: industry,
                primaryPhone: phone,
                paymentTerms: "Net 30"
            );

            // Activate some clients
            client.ChangeStatus(ClientStatus.Active);

            _context.Clients.Add(client);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} clients", clients.Count);
    }

    private async Task SeedEmployeesAsync(Guid tenantId)
    {
        // Use IgnoreQueryFilters to bypass tenant filtering during seeding
        if (await _context.Employees.IgnoreQueryFilters().AnyAsync())
        {
            _logger.LogInformation("Employees already exist, skipping employee seed");
            return;
        }

        _logger.LogInformation("Seeding test employees...");

        // Get resource types for assignment
        var resourceTypes = await _context.ResourceTypes.IgnoreQueryFilters()
            .Where(rt => rt.TenantId == tenantId)
            .ToListAsync();

        var devResourceType = resourceTypes.FirstOrDefault(rt => rt.Code == "DEV");
        var seniorDevResourceType = resourceTypes.FirstOrDefault(rt => rt.Code == "SDEV");
        var pmResourceType = resourceTypes.FirstOrDefault(rt => rt.Code == "PM");
        var qaResourceType = resourceTypes.FirstOrDefault(rt => rt.Code == "QA");
        var baResourceType = resourceTypes.FirstOrDefault(rt => rt.Code == "BA");

        if (devResourceType == null)
        {
            _logger.LogWarning("Resource types not found, skipping employee seed");
            return;
        }

        var employees = new List<(string First, string Last, string Email, string JobTitle, string Dept, long ResourceTypeId)>
        {
            ("Alice", "Johnson", "alice.johnson@company.com", "Senior Software Developer", "Engineering", seniorDevResourceType?.Id ?? devResourceType.Id),
            ("Bob", "Williams", "bob.williams@company.com", "Software Developer", "Engineering", devResourceType.Id),
            ("Carol", "Davis", "carol.davis@company.com", "Project Manager", "PMO", pmResourceType?.Id ?? devResourceType.Id),
            ("David", "Miller", "david.miller@company.com", "QA Engineer", "Quality Assurance", qaResourceType?.Id ?? devResourceType.Id),
            ("Eva", "Garcia", "eva.garcia@company.com", "Business Analyst", "Analysis", baResourceType?.Id ?? devResourceType.Id),
            ("Frank", "Martinez", "frank.martinez@company.com", "Software Developer", "Engineering", devResourceType.Id),
            ("Grace", "Anderson", "grace.anderson@company.com", "Senior Software Developer", "Engineering", seniorDevResourceType?.Id ?? devResourceType.Id),
            ("Henry", "Taylor", "henry.taylor@company.com", "Software Developer", "Engineering", devResourceType.Id),
            ("Ivy", "Thomas", "ivy.thomas@company.com", "QA Engineer", "Quality Assurance", qaResourceType?.Id ?? devResourceType.Id),
            ("Jack", "Wilson", "jack.wilson@company.com", "Project Manager", "PMO", pmResourceType?.Id ?? devResourceType.Id)
        };

        foreach (var (first, last, email, jobTitle, dept, resourceTypeId) in employees)
        {
            var employee = Employee.Create(
                tenantId,
                EmployeeNumber.Generate(),
                resourceTypeId,
                first,
                last,
                new Email(email),
                EmploymentType.FullTime,
                DateTime.UtcNow.AddYears(-1).AddDays(-Random.Shared.Next(0, 365)),
                jobTitle: jobTitle,
                department: dept,
                baseSalary: new Money(75000 + Random.Shared.Next(0, 50000), "USD"),
                standardHoursPerWeek: 40
            );

            _context.Employees.Add(employee);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} employees", employees.Count);
    }

    private async Task SeedProjectsAsync(Guid tenantId)
    {
        // Use IgnoreQueryFilters to bypass tenant filtering during seeding
        if (await _context.Projects.IgnoreQueryFilters().AnyAsync())
        {
            _logger.LogInformation("Projects already exist, skipping project seed");
            return;
        }

        _logger.LogInformation("Seeding test projects...");

        // Get clients for project assignment
        var clients = await _context.Clients.IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId)
            .Take(5)
            .ToListAsync();

        if (!clients.Any())
        {
            _logger.LogWarning("No clients found, skipping project seed");
            return;
        }

        var projects = new List<(string Name, string Description, ProjectType Type, BillingMode Billing, int ClientIndex)>
        {
            ("Website Redesign", "Complete redesign of corporate website with modern UI/UX", ProjectType.Billable, BillingMode.FixedPrice, 0),
            ("Mobile App Development", "Native iOS and Android app development", ProjectType.Billable, BillingMode.TimeAndMaterials, 1),
            ("ERP Implementation", "Enterprise resource planning system implementation", ProjectType.Billable, BillingMode.TimeAndMaterials, 2),
            ("Data Migration", "Legacy system data migration to cloud platform", ProjectType.Billable, BillingMode.FixedPrice, 3),
            ("Security Audit", "Comprehensive security assessment and remediation", ProjectType.Billable, BillingMode.TimeAndMaterials, 4),
            ("Internal Tools", "Development of internal productivity tools", ProjectType.Internal, BillingMode.NonBillable, 0),
            ("Training Program", "Employee technical training program", ProjectType.Overhead, BillingMode.NonBillable, 0),
            ("API Integration", "Third-party API integration project", ProjectType.Billable, BillingMode.TimeAndMaterials, 1),
            ("Cloud Migration", "On-premise to cloud infrastructure migration", ProjectType.Billable, BillingMode.Milestone, 2),
            ("Support Contract", "Ongoing technical support and maintenance", ProjectType.Billable, BillingMode.TimeAndMaterials, 3)
        };

        foreach (var (name, description, type, billing, clientIndex) in projects)
        {
            var client = clients[clientIndex % clients.Count];
            var startDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(30, 180));

            var project = Project.Create(
                tenantId,
                ProjectNumber.Generate(),
                client.Id,
                name,
                type,
                billing,
                startDate,
                description: description,
                endDate: startDate.AddMonths(Random.Shared.Next(3, 12)),
                budget: type == ProjectType.Billable ? new Money(50000 + Random.Shared.Next(0, 200000), "USD") : null
            );

            // Set some projects to active status
            if (Random.Shared.Next(0, 2) == 0)
            {
                project.ChangeStatus(ProjectStatus.Active);
            }

            _context.Projects.Add(project);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} projects", projects.Count);
    }
}
