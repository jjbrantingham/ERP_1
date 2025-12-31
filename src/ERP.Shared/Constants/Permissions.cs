namespace ERP.Shared.Constants;

/// <summary>
/// Defines all permissions in the system.
/// Permissions follow the pattern: "{Module}.{Action}".
/// </summary>
public static class Permissions
{
    // User Management
    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersEdit = "Users.Edit";
    public const string UsersDelete = "Users.Delete";
    public const string UsersManageRoles = "Users.ManageRoles";

    // Role Management
    public const string RolesView = "Roles.View";
    public const string RolesCreate = "Roles.Create";
    public const string RolesEdit = "Roles.Edit";
    public const string RolesDelete = "Roles.Delete";
    public const string RolesManagePermissions = "Roles.ManagePermissions";

    // Project Management
    public const string ProjectsView = "Projects.View";
    public const string ProjectsCreate = "Projects.Create";
    public const string ProjectsEdit = "Projects.Edit";
    public const string ProjectsDelete = "Projects.Delete";
    public const string ProjectsManageWBS = "Projects.ManageWBS";
    public const string ProjectsManageResources = "Projects.ManageResources";

    // Client Management (CRM)
    public const string ClientsView = "Clients.View";
    public const string ClientsCreate = "Clients.Create";
    public const string ClientsEdit = "Clients.Edit";
    public const string ClientsDelete = "Clients.Delete";

    // Employee Management (HR)
    public const string EmployeesView = "Employees.View";
    public const string EmployeesCreate = "Employees.Create";
    public const string EmployeesEdit = "Employees.Edit";
    public const string EmployeesDelete = "Employees.Delete";
    public const string EmployeesManageRates = "Employees.ManageRates";

    // Timesheet Management
    public const string TimesheetsView = "Timesheets.View";
    public const string TimesheetsCreate = "Timesheets.Create";
    public const string TimesheetsEdit = "Timesheets.Edit";
    public const string TimesheetsDelete = "Timesheets.Delete";
    public const string TimesheetsSubmit = "Timesheets.Submit";
    public const string TimesheetsApprove = "Timesheets.Approve";
    public const string TimesheetsReject = "Timesheets.Reject";

    // Expense Management
    public const string ExpensesView = "Expenses.View";
    public const string ExpensesCreate = "Expenses.Create";
    public const string ExpensesEdit = "Expenses.Edit";
    public const string ExpensesDelete = "Expenses.Delete";
    public const string ExpensesSubmit = "Expenses.Submit";
    public const string ExpensesApprove = "Expenses.Approve";
    public const string ExpensesReject = "Expenses.Reject";

    // Financial Management
    public const string AccountsView = "Accounts.View";
    public const string AccountsCreate = "Accounts.Create";
    public const string AccountsEdit = "Accounts.Edit";
    public const string AccountsDelete = "Accounts.Delete";
    public const string JournalEntriesView = "JournalEntries.View";
    public const string JournalEntriesCreate = "JournalEntries.Create";
    public const string JournalEntriesPost = "JournalEntries.Post";

    // Billing & Invoicing
    public const string InvoicesView = "Invoices.View";
    public const string InvoicesCreate = "Invoices.Create";
    public const string InvoicesEdit = "Invoices.Edit";
    public const string InvoicesDelete = "Invoices.Delete";
    public const string InvoicesSend = "Invoices.Send";
    public const string InvoicesVoid = "Invoices.Void";
    public const string PaymentsRecord = "Payments.Record";

    // Reporting
    public const string ReportsView = "Reports.View";
    public const string ReportsExport = "Reports.Export";
    public const string ReportsSchedule = "Reports.Schedule";
    public const string DashboardsView = "Dashboards.View";
    public const string DashboardsCustomize = "Dashboards.Customize";

    // Vendor Management
    public const string VendorsView = "Vendors.View";
    public const string VendorsCreate = "Vendors.Create";
    public const string VendorsEdit = "Vendors.Edit";
    public const string VendorsDelete = "Vendors.Delete";

    // System Administration
    public const string SystemSettings = "System.Settings";
    public const string SystemAuditLogs = "System.AuditLogs";
    public const string SystemBackup = "System.Backup";
    public const string TenantManage = "Tenant.Manage";

    /// <summary>
    /// Gets all permissions defined in the system.
    /// </summary>
    public static IEnumerable<string> GetAllPermissions()
    {
        return typeof(Permissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => (string)fi.GetValue(null)!)
            .OrderBy(p => p);
    }

    /// <summary>
    /// Gets all permissions for a specific module.
    /// </summary>
    public static IEnumerable<string> GetModulePermissions(string module)
    {
        return GetAllPermissions().Where(p => p.StartsWith($"{module}."));
    }
}
