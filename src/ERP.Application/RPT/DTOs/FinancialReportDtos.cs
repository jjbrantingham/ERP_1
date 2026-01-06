namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Profit and Loss (Income Statement) report
/// </summary>
public class ProfitAndLossDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<RevenueLineDto> Revenue { get; set; } = new();
    public List<ExpenseLineDto> Expenses { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal OperatingIncome { get; set; }
}

/// <summary>
/// Revenue line in P&L
/// </summary>
public class RevenueLineDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// Expense line in P&L
/// </summary>
public class ExpenseLineDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// Cash Flow Statement report
/// </summary>
public class CashFlowDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CashFlowLineDto> OperatingActivities { get; set; } = new();
    public List<CashFlowLineDto> InvestingActivities { get; set; } = new();
    public List<CashFlowLineDto> FinancingActivities { get; set; } = new();
    public decimal NetCashFromOperating { get; set; }
    public decimal NetCashFromInvesting { get; set; }
    public decimal NetCashFromFinancing { get; set; }
    public decimal NetChangeInCash { get; set; }
    public decimal BeginningCashBalance { get; set; }
    public decimal EndingCashBalance { get; set; }
}

/// <summary>
/// Cash flow line item
/// </summary>
public class CashFlowLineDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// AR Aging report
/// </summary>
public class ARAgingDto
{
    public DateTime AsOfDate { get; set; }
    public List<ARAgingLineDto> Lines { get; set; } = new();
    public decimal TotalCurrent { get; set; }
    public decimal Total1To30Days { get; set; }
    public decimal Total31To60Days { get; set; }
    public decimal Total61To90Days { get; set; }
    public decimal TotalOver90Days { get; set; }
    public decimal TotalAR { get; set; }
}

/// <summary>
/// AR Aging line item
/// </summary>
public class ARAgingLineDto
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOutstanding { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90Days { get; set; }
}
