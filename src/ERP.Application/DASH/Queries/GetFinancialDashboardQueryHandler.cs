using ERP.Application.Common.Interfaces;
using ERP.Application.DASH.DTOs;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.DASH.Enums;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using MediatR;

namespace ERP.Application.DASH.Queries;

/// <summary>
/// Handler for getting financial dashboard data
/// </summary>
public class GetFinancialDashboardQueryHandler : IRequestHandler<GetFinancialDashboardQuery, FinancialDashboardDto>
{
    private readonly ICurrentTenantService _currentTenant;
    private readonly IAccountRepository _accountRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public GetFinancialDashboardQueryHandler(
        ICurrentTenantService currentTenant,
        IAccountRepository accountRepository,
        IJournalEntryRepository journalEntryRepository,
        IInvoiceRepository invoiceRepository)
    {
        _currentTenant = currentTenant;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<FinancialDashboardDto> Handle(GetFinancialDashboardQuery request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate ?? DateTime.UtcNow;
        var currentMonth = new DateTime(asOfDate.Year, asOfDate.Month, 1);
        var previousMonth = currentMonth.AddMonths(-1);
        var currency = request.Currency;

        var dashboard = new FinancialDashboardDto
        {
            AsOfDate = asOfDate,
            TenantId = _currentTenant.TenantId
        };

        // Get all financial data
        var accounts = (await _accountRepository.GetAllAsync(cancellationToken)).ToList();
        var invoices = (await _invoiceRepository.GetAllAsync(cancellationToken)).ToList();

        // Calculate all metrics
        CalculateCashFlowMetrics(dashboard, accounts, currentMonth, previousMonth, currency);
        CalculateRevenueMetrics(dashboard, invoices, currentMonth, previousMonth, currency);
        CalculateProfitabilityMetrics(dashboard, accounts, invoices, currentMonth, currency);
        CalculateAccountsReceivableMetrics(dashboard, invoices, asOfDate, currency);
        CalculateAccountsPayableMetrics(dashboard, accounts, asOfDate, currency);
        CalculateInvoicingMetrics(dashboard, invoices, currentMonth);
        GenerateChartData(dashboard, accounts, invoices, asOfDate, currency);

        return dashboard;
    }

    private void CalculateCashFlowMetrics(
        FinancialDashboardDto dashboard,
        List<ERP.Domain.FIN.Entities.Account> accounts,
        DateTime currentMonth,
        DateTime previousMonth,
        string currency)
    {
        // Cash balance (current assets - typically accounts starting with 10)
        var cashBalance = accounts
            .Where(a => a.Type == AccountType.Asset && a.AccountNumber.Value.StartsWith("10"))
            .Sum(a => a.Balance);

        dashboard.CashBalance = CreateKpiMetric(
            KpiType.CashFlow,
            "Cash Balance",
            cashBalance,
            null,
            null,
            currency,
            "currency");

        // Monthly inflow/outflow (simplified - would come from cash flow statement)
        var monthlyInflow = cashBalance * 0.15m; // Placeholder
        var monthlyOutflow = cashBalance * 0.12m; // Placeholder
        var netCashFlow = monthlyInflow - monthlyOutflow;

        dashboard.MonthlyInflow = CreateKpiMetric(
            KpiType.CashFlow,
            "Monthly Inflow",
            monthlyInflow,
            null,
            null,
            currency,
            "currency");

        dashboard.MonthlyOutflow = CreateKpiMetric(
            KpiType.CashFlow,
            "Monthly Outflow",
            monthlyOutflow,
            null,
            null,
            currency,
            "currency");

        dashboard.NetCashFlow = CreateKpiMetric(
            KpiType.CashFlow,
            "Net Cash Flow",
            netCashFlow,
            null,
            null,
            currency,
            "currency");

        // Cash runway (months of operation based on current cash)
        var cashRunway = monthlyOutflow > 0 ? cashBalance / monthlyOutflow : 999;
        dashboard.CashRunwayMonths = CreateKpiMetric(
            KpiType.CashFlow,
            "Cash Runway (Months)",
            cashRunway,
            null,
            6m, // Target: 6 months runway
            null,
            "number");
    }

    private void CalculateRevenueMetrics(
        FinancialDashboardDto dashboard,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        DateTime currentMonth,
        DateTime previousMonth,
        string currency)
    {
        var currentMonthInvoices = invoices
            .Where(i => i.InvoiceDate >= currentMonth && i.InvoiceDate < currentMonth.AddMonths(1) &&
                       (i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.Paid))
            .ToList();

        var previousMonthInvoices = invoices
            .Where(i => i.InvoiceDate >= previousMonth && i.InvoiceDate < currentMonth &&
                       (i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.Paid))
            .ToList();

        var currentRevenue = currentMonthInvoices.Sum(i => i.CalculateTotal().Amount);
        var previousRevenue = previousMonthInvoices.Sum(i => i.CalculateTotal().Amount);

        dashboard.TotalRevenue = CreateKpiMetric(
            KpiType.Revenue,
            "Total Revenue",
            currentRevenue,
            previousRevenue,
            null,
            currency,
            "currency");

        // Recurring vs project revenue (simplified)
        var recurringRevenue = currentRevenue * 0.3m; // Placeholder
        var projectRevenue = currentRevenue * 0.7m; // Placeholder

        dashboard.RecurringRevenue = CreateKpiMetric(
            KpiType.Revenue,
            "Recurring Revenue",
            recurringRevenue,
            null,
            null,
            currency,
            "currency");

        dashboard.ProjectRevenue = CreateKpiMetric(
            KpiType.Revenue,
            "Project Revenue",
            projectRevenue,
            null,
            null,
            currency,
            "currency");

        // Revenue growth
        var revenueGrowth = previousRevenue > 0 ? ((currentRevenue - previousRevenue) / previousRevenue) * 100 : 0;
        dashboard.RevenueGrowth = CreateKpiMetric(
            KpiType.Revenue,
            "Revenue Growth",
            revenueGrowth,
            null,
            10m, // Target: 10% growth
            null,
            "percentage");
    }

    private void CalculateProfitabilityMetrics(
        FinancialDashboardDto dashboard,
        List<ERP.Domain.FIN.Entities.Account> accounts,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        DateTime currentMonth,
        string currency)
    {
        var currentMonthRevenue = invoices
            .Where(i => i.InvoiceDate >= currentMonth && i.InvoiceDate < currentMonth.AddMonths(1) &&
                       (i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.Paid))
            .Sum(i => i.CalculateTotal().Amount);

        // Get expenses from account balances
        var totalExpenses = accounts
            .Where(a => a.Type == AccountType.Expense)
            .Sum(a => a.Balance);

        var costOfGoodsSold = totalExpenses * 0.4m; // Simplified - would use specific accounts
        var operatingExpenses = totalExpenses * 0.6m;

        // Gross profit
        var grossProfit = currentMonthRevenue - costOfGoodsSold;
        var grossProfitMargin = currentMonthRevenue > 0 ? (grossProfit / currentMonthRevenue) * 100 : 0;

        dashboard.GrossProfit = CreateKpiMetric(
            KpiType.Profit,
            "Gross Profit",
            grossProfit,
            null,
            null,
            currency,
            "currency");

        dashboard.GrossProfitMargin = CreateKpiMetric(
            KpiType.Profit,
            "Gross Profit Margin",
            grossProfitMargin,
            null,
            50m, // Target: 50% margin
            null,
            "percentage");

        // Operating profit
        var operatingProfit = grossProfit - operatingExpenses;
        var operatingMargin = currentMonthRevenue > 0 ? (operatingProfit / currentMonthRevenue) * 100 : 0;

        dashboard.OperatingProfit = CreateKpiMetric(
            KpiType.Profit,
            "Operating Profit",
            operatingProfit,
            null,
            null,
            currency,
            "currency");

        dashboard.OperatingMargin = CreateKpiMetric(
            KpiType.Profit,
            "Operating Margin",
            operatingMargin,
            null,
            30m, // Target: 30% margin
            null,
            "percentage");

        // Net profit (same as operating for now - would subtract taxes, interest, etc.)
        var netProfit = operatingProfit;
        var netProfitMargin = operatingMargin;

        dashboard.NetProfit = CreateKpiMetric(
            KpiType.Profit,
            "Net Profit",
            netProfit,
            null,
            null,
            currency,
            "currency");

        dashboard.NetProfitMargin = CreateKpiMetric(
            KpiType.Profit,
            "Net Profit Margin",
            netProfitMargin,
            null,
            20m, // Target: 20% margin
            null,
            "percentage");
    }

    private void CalculateAccountsReceivableMetrics(
        FinancialDashboardDto dashboard,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        DateTime asOfDate,
        string currency)
    {
        var outstandingInvoices = invoices
            .Where(i => i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.PartiallyPaid)
            .ToList();

        var totalAR = outstandingInvoices.Sum(i =>
        {
            var total = i.CalculateTotal();
            return total.Amount - i.AmountPaid.Amount;
        });

        dashboard.TotalAR = CreateKpiMetric(
            KpiType.AccountsReceivable,
            "Total AR",
            totalAR,
            null,
            null,
            currency,
            "currency");

        // AR aging
        var current = outstandingInvoices
            .Where(i => (asOfDate - i.DueDate).Days <= 0)
            .Sum(i => i.CalculateTotal().Amount - i.AmountPaid.Amount);

        var ar30 = outstandingInvoices
            .Where(i => (asOfDate - i.DueDate).Days > 0 && (asOfDate - i.DueDate).Days <= 30)
            .Sum(i => i.CalculateTotal().Amount - i.AmountPaid.Amount);

        var ar60 = outstandingInvoices
            .Where(i => (asOfDate - i.DueDate).Days > 30 && (asOfDate - i.DueDate).Days <= 60)
            .Sum(i => i.CalculateTotal().Amount - i.AmountPaid.Amount);

        var ar90 = outstandingInvoices
            .Where(i => (asOfDate - i.DueDate).Days > 60 && (asOfDate - i.DueDate).Days <= 90)
            .Sum(i => i.CalculateTotal().Amount - i.AmountPaid.Amount);

        var arOver90 = outstandingInvoices
            .Where(i => (asOfDate - i.DueDate).Days > 90)
            .Sum(i => i.CalculateTotal().Amount - i.AmountPaid.Amount);

        dashboard.CurrentAR = CreateKpiMetric(KpiType.AccountsReceivable, "Current AR", current, null, null, currency, "currency");
        dashboard.AR30Days = CreateKpiMetric(KpiType.AccountsReceivable, "AR 1-30 Days", ar30, null, null, currency, "currency");
        dashboard.AR60Days = CreateKpiMetric(KpiType.AccountsReceivable, "AR 31-60 Days", ar60, null, null, currency, "currency");
        dashboard.AR90Days = CreateKpiMetric(KpiType.AccountsReceivable, "AR 61-90 Days", ar90, null, null, currency, "currency");
        dashboard.AROver90Days = CreateKpiMetric(KpiType.AccountsReceivable, "AR Over 90 Days", arOver90, null, null, currency, "currency");

        // Average days to collect
        var avgDays = outstandingInvoices.Count > 0
            ? outstandingInvoices.Average(i => (asOfDate - i.InvoiceDate).Days)
            : 0;

        dashboard.AverageDaysToCollect = CreateKpiMetric(
            KpiType.AccountsReceivable,
            "Avg Days to Collect",
            (decimal)avgDays,
            null,
            30m, // Target: 30 days
            null,
            "number");
    }

    private void CalculateAccountsPayableMetrics(
        FinancialDashboardDto dashboard,
        List<ERP.Domain.FIN.Entities.Account> accounts,
        DateTime asOfDate,
        string currency)
    {
        // Simplified - would come from actual AP tracking
        var totalAP = accounts
            .Where(a => a.Type == AccountType.Liability && a.AccountNumber.Value.StartsWith("20"))
            .Sum(a => a.Balance);

        dashboard.TotalAP = CreateKpiMetric(
            KpiType.AccountsPayable,
            "Total AP",
            totalAP,
            null,
            null,
            currency,
            "currency");

        dashboard.CurrentAP = CreateKpiMetric(
            KpiType.AccountsPayable,
            "Current AP",
            totalAP * 0.7m,
            null,
            null,
            currency,
            "currency");

        dashboard.OverdueAP = CreateKpiMetric(
            KpiType.AccountsPayable,
            "Overdue AP",
            totalAP * 0.3m,
            null,
            null,
            currency,
            "currency");
    }

    private void CalculateInvoicingMetrics(
        FinancialDashboardDto dashboard,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        DateTime currentMonth)
    {
        var currentMonthInvoices = invoices
            .Where(i => i.InvoiceDate >= currentMonth && i.InvoiceDate < currentMonth.AddMonths(1))
            .ToList();

        var invoicesSent = currentMonthInvoices.Count(i => i.Status != InvoiceStatus.Draft);
        var invoicesPaid = currentMonthInvoices.Count(i => i.Status == InvoiceStatus.Paid);
        var invoicesOverdue = currentMonthInvoices.Count(i =>
            (i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.PartiallyPaid) &&
            i.DueDate < DateTime.UtcNow);

        dashboard.InvoicesSent = CreateKpiMetric(KpiType.Revenue, "Invoices Sent", invoicesSent, null, null, null, "number");
        dashboard.InvoicesPaid = CreateKpiMetric(KpiType.Revenue, "Invoices Paid", invoicesPaid, null, null, null, "number");
        dashboard.InvoicesOverdue = CreateKpiMetric(KpiType.Revenue, "Invoices Overdue", invoicesOverdue, null, null, null, "number");

        var avgInvoiceValue = currentMonthInvoices.Count > 0
            ? currentMonthInvoices.Average(i => i.CalculateTotal().Amount)
            : 0;

        dashboard.AverageInvoiceValue = CreateKpiMetric(
            KpiType.Revenue,
            "Avg Invoice Value",
            avgInvoiceValue,
            null,
            null,
            "USD",
            "currency");
    }

    private void GenerateChartData(
        FinancialDashboardDto dashboard,
        List<ERP.Domain.FIN.Entities.Account> accounts,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        DateTime asOfDate,
        string currency)
    {
        // Cash flow trend (last 6 months)
        dashboard.CashFlowTrend = new List<ChartDataPointDto>();
        for (int i = 5; i >= 0; i--)
        {
            var month = asOfDate.AddMonths(-i);
            var monthStart = new DateTime(month.Year, month.Month, 1);

            dashboard.CashFlowTrend.Add(new ChartDataPointDto
            {
                Label = monthStart.ToString("MMM yyyy"),
                Date = monthStart,
                Value = 50000 + (i * 5000), // Placeholder
                SecondaryValue = 45000 + (i * 4000) // Placeholder
            });
        }

        // Revenue trend (last 12 months)
        dashboard.RevenueTrend = new List<ChartDataPointDto>();
        for (int i = 11; i >= 0; i--)
        {
            var month = asOfDate.AddMonths(-i);
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var monthRevenue = invoices
                .Where(inv => inv.InvoiceDate >= monthStart && inv.InvoiceDate < monthEnd &&
                             (inv.Status == InvoiceStatus.Posted || inv.Status == InvoiceStatus.Paid))
                .Sum(inv => inv.CalculateTotal().Amount);

            dashboard.RevenueTrend.Add(new ChartDataPointDto
            {
                Label = monthStart.ToString("MMM yyyy"),
                Date = monthStart,
                Value = monthRevenue
            });
        }

        // Revenue by project (top 5)
        dashboard.RevenueByProject = new List<PieChartDataDto>();
        // Would aggregate by project - simplified for now

        // Expense by category
        dashboard.ExpenseByCategory = new List<PieChartDataDto>();
        // Would aggregate by expense account category
    }

    private KpiMetricDto CreateKpiMetric(
        KpiType type,
        string name,
        decimal currentValue,
        decimal? previousValue,
        decimal? targetValue,
        string? currency,
        string format)
    {
        var percentageChange = previousValue.HasValue && previousValue.Value != 0
            ? ((currentValue - previousValue.Value) / previousValue.Value) * 100
            : (decimal?)null;

        var trend = TrendDirection.Flat;
        if (percentageChange.HasValue)
        {
            if (percentageChange.Value > 2) trend = TrendDirection.Up;
            else if (percentageChange.Value < -2) trend = TrendDirection.Down;
        }

        var isHealthy = true;
        if (targetValue.HasValue)
        {
            isHealthy = currentValue >= targetValue.Value;
        }

        return new KpiMetricDto
        {
            Type = type,
            Name = name,
            CurrentValue = Math.Round(currentValue, 2),
            PreviousValue = previousValue.HasValue ? Math.Round(previousValue.Value, 2) : null,
            TargetValue = targetValue.HasValue ? Math.Round(targetValue.Value, 2) : null,
            PercentageChange = percentageChange.HasValue ? Math.Round(percentageChange.Value, 2) : null,
            Trend = trend,
            Currency = currency,
            Format = format,
            IsHealthy = isHealthy
        };
    }
}
