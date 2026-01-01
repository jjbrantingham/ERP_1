namespace ERP.Domain.RPT.Enums;

/// <summary>
/// Types of reports available in the system
/// </summary>
public enum ReportType : byte
{
    // Financial Reports
    BalanceSheet = 1,
    IncomeStatement = 2,
    CashFlowStatement = 3,
    TrialBalance = 4,
    GeneralLedger = 5,
    AccountsReceivableAging = 6,
    AccountsPayableAging = 7,

    // Project Reports
    ProjectProfitability = 10,
    ProjectStatus = 11,
    ResourceUtilization = 12,
    TimeAndExpenseSummary = 13,

    // Operational Reports
    TimesheetSummary = 20,
    ExpenseSummary = 21,
    InvoiceSummary = 22,
    PaymentSummary = 23,
    VendorSummary = 24,

    // Custom Reports
    Custom = 99
}
