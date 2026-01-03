namespace ERP.Domain.RPT.Enums;

/// <summary>
/// Type of report
/// </summary>
public enum ReportType : byte
{
    /// <summary>
    /// Financial report
    /// </summary>
    Financial = 1,

    /// <summary>
    /// Project report
    /// </summary>
    Project = 2,

    /// <summary>
    /// Operational report
    /// </summary>
    Operational = 3,

    /// <summary>
    /// Dashboard
    /// </summary>
    Dashboard = 4,

    /// <summary>
    /// Custom report
    /// </summary>
    Custom = 99
}

/// <summary>
/// Report export format
/// </summary>
public enum ExportFormat : byte
{
    /// <summary>
    /// PDF format
    /// </summary>
    PDF = 1,

    /// <summary>
    /// Excel format (.xlsx)
    /// </summary>
    Excel = 2,

    /// <summary>
    /// CSV format
    /// </summary>
    CSV = 3,

    /// <summary>
    /// JSON format
    /// </summary>
    JSON = 4,

    /// <summary>
    /// HTML format
    /// </summary>
    HTML = 5
}

/// <summary>
/// Report period type
/// </summary>
public enum ReportPeriod : byte
{
    /// <summary>
    /// Custom date range
    /// </summary>
    Custom = 0,

    /// <summary>
    /// Today
    /// </summary>
    Today = 1,

    /// <summary>
    /// This week
    /// </summary>
    ThisWeek = 2,

    /// <summary>
    /// This month
    /// </summary>
    ThisMonth = 3,

    /// <summary>
    /// This quarter
    /// </summary>
    ThisQuarter = 4,

    /// <summary>
    /// This year
    /// </summary>
    ThisYear = 5,

    /// <summary>
    /// Last month
    /// </summary>
    LastMonth = 6,

    /// <summary>
    /// Last quarter
    /// </summary>
    LastQuarter = 7,

    /// <summary>
    /// Last year
    /// </summary>
    LastYear = 8,

    /// <summary>
    /// Year to date
    /// </summary>
    YearToDate = 9,

    /// <summary>
    /// Quarter to date
    /// </summary>
    QuarterToDate = 10
}

/// <summary>
/// Financial statement type
/// </summary>
public enum FinancialStatementType : byte
{
    /// <summary>
    /// Profit and Loss (Income Statement)
    /// </summary>
    ProfitAndLoss = 1,

    /// <summary>
    /// Balance Sheet
    /// </summary>
    BalanceSheet = 2,

    /// <summary>
    /// Cash Flow Statement
    /// </summary>
    CashFlow = 3,

    /// <summary>
    /// AR Aging
    /// </summary>
    ARAge = 4,

    /// <summary>
    /// AP Aging
    /// </summary>
    APAging = 5,

    /// <summary>
    /// Trial Balance
    /// </summary>
    TrialBalance = 6,

    /// <summary>
    /// General Ledger
    /// </summary>
    GeneralLedger = 7
}

/// <summary>
/// Project report type
/// </summary>
public enum ProjectReportType : byte
{
    /// <summary>
    /// Project status report
    /// </summary>
    ProjectStatus = 1,

    /// <summary>
    /// Project profitability
    /// </summary>
    Profitability = 2,

    /// <summary>
    /// Resource utilization
    /// </summary>
    ResourceUtilization = 3,

    /// <summary>
    /// Budget variance
    /// </summary>
    BudgetVariance = 4,

    /// <summary>
    /// Project timeline
    /// </summary>
    Timeline = 5,

    /// <summary>
    /// Milestone tracking
    /// </summary>
    MilestoneTracking = 6
}
