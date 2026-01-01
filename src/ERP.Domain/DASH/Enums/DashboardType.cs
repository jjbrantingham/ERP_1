namespace ERP.Domain.DASH.Enums;

/// <summary>
/// Types of dashboards available in the system
/// </summary>
public enum DashboardType : byte
{
    /// <summary>
    /// Executive/CEO dashboard with high-level KPIs
    /// </summary>
    Executive = 1,

    /// <summary>
    /// Financial dashboard with cash flow, AR/AP, etc.
    /// </summary>
    Financial = 2,

    /// <summary>
    /// Project Manager dashboard with project metrics
    /// </summary>
    ProjectManager = 3,

    /// <summary>
    /// Employee dashboard with personal timesheets and expenses
    /// </summary>
    Employee = 4,

    /// <summary>
    /// Operations dashboard with utilization and capacity
    /// </summary>
    Operations = 5
}
