namespace ERP.Domain.DASH.Enums;

/// <summary>
/// Types of Key Performance Indicators (KPIs)
/// </summary>
public enum KpiType : byte
{
    // Financial KPIs
    Revenue = 1,
    Profit = 2,
    CashFlow = 3,
    AccountsReceivable = 4,
    AccountsPayable = 5,

    // Project KPIs
    ActiveProjects = 10,
    ProjectProfitability = 11,
    ProjectUtilization = 12,
    OverBudgetProjects = 13,

    // Employee KPIs
    EmployeeUtilization = 20,
    BillableHours = 21,
    NonBillableHours = 22,
    OverdueTimesheets = 23,

    // Operational KPIs
    Capacity = 30,
    AvailableResources = 31,
    ClientSatisfaction = 32,
    InvoiceCollectionRate = 33
}
