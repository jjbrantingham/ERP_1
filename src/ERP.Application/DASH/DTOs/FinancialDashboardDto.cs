namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Financial dashboard with detailed financial metrics
/// </summary>
public class FinancialDashboardDto
{
    /// <summary>
    /// As of date for the dashboard data
    /// </summary>
    public DateTime AsOfDate { get; set; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    public Guid TenantId { get; set; }

    // Cash Flow Metrics
    public KpiMetricDto CashBalance { get; set; } = new();
    public KpiMetricDto MonthlyInflow { get; set; } = new();
    public KpiMetricDto MonthlyOutflow { get; set; } = new();
    public KpiMetricDto NetCashFlow { get; set; } = new();
    public KpiMetricDto CashRunwayMonths { get; set; } = new();

    // Revenue Metrics
    public KpiMetricDto TotalRevenue { get; set; } = new();
    public KpiMetricDto RecurringRevenue { get; set; } = new();
    public KpiMetricDto ProjectRevenue { get; set; } = new();
    public KpiMetricDto RevenueGrowth { get; set; } = new();

    // Profitability Metrics
    public KpiMetricDto GrossProfit { get; set; } = new();
    public KpiMetricDto GrossProfitMargin { get; set; } = new();
    public KpiMetricDto OperatingProfit { get; set; } = new();
    public KpiMetricDto OperatingMargin { get; set; } = new();
    public KpiMetricDto NetProfit { get; set; } = new();
    public KpiMetricDto NetProfitMargin { get; set; } = new();

    // Accounts Receivable
    public KpiMetricDto TotalAR { get; set; } = new();
    public KpiMetricDto CurrentAR { get; set; } = new();
    public KpiMetricDto AR30Days { get; set; } = new();
    public KpiMetricDto AR60Days { get; set; } = new();
    public KpiMetricDto AR90Days { get; set; } = new();
    public KpiMetricDto AROver90Days { get; set; } = new();
    public KpiMetricDto AverageDaysToCollect { get; set; } = new();

    // Accounts Payable
    public KpiMetricDto TotalAP { get; set; } = new();
    public KpiMetricDto CurrentAP { get; set; } = new();
    public KpiMetricDto OverdueAP { get; set; } = new();

    // Invoicing Metrics
    public KpiMetricDto InvoicesSent { get; set; } = new();
    public KpiMetricDto InvoicesPaid { get; set; } = new();
    public KpiMetricDto InvoicesOverdue { get; set; } = new();
    public KpiMetricDto AverageInvoiceValue { get; set; } = new();

    // Chart data for trends
    public List<ChartDataPointDto> CashFlowTrend { get; set; } = new();
    public List<ChartDataPointDto> RevenueTrend { get; set; } = new();
    public List<ChartDataPointDto> ProfitTrend { get; set; } = new();
    public List<PieChartDataDto> RevenueByProject { get; set; } = new();
    public List<PieChartDataDto> ExpenseByCategory { get; set; } = new();
}
