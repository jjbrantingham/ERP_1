namespace ERP.Shared.Constants;

/// <summary>
/// Defines standard system roles.
/// </summary>
public static class Roles
{
    /// <summary>
    /// System Administrator - full access to all system functions.
    /// </summary>
    public const string SystemAdministrator = "System Administrator";

    /// <summary>
    /// Tenant Administrator - full access within the tenant.
    /// </summary>
    public const string TenantAdministrator = "Tenant Administrator";

    /// <summary>
    /// Project Manager - manages projects, resources, and schedules.
    /// </summary>
    public const string ProjectManager = "Project Manager";

    /// <summary>
    /// Finance Manager - manages financial operations, billing, and accounting.
    /// </summary>
    public const string FinanceManager = "Finance Manager";

    /// <summary>
    /// HR Manager - manages employees, rates, and HR operations.
    /// </summary>
    public const string HRManager = "HR Manager";

    /// <summary>
    /// Employee - standard employee with time and expense entry capabilities.
    /// </summary>
    public const string Employee = "Employee";

    /// <summary>
    /// Client - external user with limited access to their projects and invoices.
    /// </summary>
    public const string Client = "Client";

    /// <summary>
    /// Accountant - manages accounting, ledger, and financial reports.
    /// </summary>
    public const string Accountant = "Accountant";

    /// <summary>
    /// Viewer - read-only access to reports and dashboards.
    /// </summary>
    public const string Viewer = "Viewer";

    /// <summary>
    /// Gets all standard system roles.
    /// </summary>
    public static IEnumerable<string> GetAllRoles()
    {
        return new[]
        {
            SystemAdministrator,
            TenantAdministrator,
            ProjectManager,
            FinanceManager,
            HRManager,
            Employee,
            Client,
            Accountant,
            Viewer
        };
    }
}
