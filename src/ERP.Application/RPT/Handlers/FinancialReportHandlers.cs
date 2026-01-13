using ERP.Application.RPT.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.RPT.Queries;
using ERP.Application.RPT.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.RPT.Handlers;

/// <summary>
/// Handler for Profit and Loss report
/// </summary>
public class GetProfitAndLossQueryHandler : IRequestHandler<GetProfitAndLossQuery, ProfitAndLossDto>
{
    private readonly IDbContext _context;
    private readonly IReportPeriodService _periodService;

    public GetProfitAndLossQueryHandler(IDbContext context, IReportPeriodService periodService)
    {
        _context = context;
        _periodService = periodService;
    }

    public async Task<ProfitAndLossDto> Handle(GetProfitAndLossQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = _periodService.GetDateRange(request.Period, request.StartDate, request.EndDate);

        // Get all journal entry lines in the period
        var journalLines = await _context.JournalEntryLines
            .Where(l => l.JournalEntry.EntryDate >= startDate && l.JournalEntry.EntryDate <= endDate)
            .Where(l => l.JournalEntry.Status == ERP.Domain.FIN.Enums.JournalEntryStatus.Posted)
            .ToListAsync(cancellationToken);

        // Get accounts for these lines
        var accountIds = journalLines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = await _context.Accounts
            .Where(a => accountIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        var accountDict = accounts.ToDictionary(a => a.Id);

        var report = new ProfitAndLossDto
        {
            StartDate = startDate,
            EndDate = endDate
        };

        // Revenue accounts (4000-4999)
        report.Revenue = journalLines
            .Where(l => accountDict.TryGetValue(l.AccountId, out var acc) && acc.AccountNumber.Value.StartsWith("4"))
            .GroupBy(l => accountDict[l.AccountId])
            .Select(g => new RevenueLineDto
            {
                AccountNumber = g.Key.AccountNumber.Value,
                AccountName = g.Key.Name,
                Amount = g.Sum(l => l.CreditAmount - l.DebitAmount) // Revenue is credit balance
            })
            .Where(l => l.Amount != 0)
            .OrderBy(l => l.AccountNumber)
            .ToList();

        // Expense accounts (6000-9999)
        report.Expenses = journalLines
            .Where(l => accountDict.TryGetValue(l.AccountId, out var acc) && int.Parse(acc.AccountNumber.Value.Substring(0, 1)) >= 6)
            .GroupBy(l => accountDict[l.AccountId])
            .Select(g => new ExpenseLineDto
            {
                AccountNumber = g.Key.AccountNumber.Value,
                AccountName = g.Key.Name,
                Amount = g.Sum(l => l.DebitAmount - l.CreditAmount) // Expenses are debit balance
            })
            .Where(l => l.Amount != 0)
            .OrderBy(l => l.AccountNumber)
            .ToList();

        report.TotalRevenue = report.Revenue.Sum(r => r.Amount);
        report.TotalExpenses = report.Expenses.Sum(e => e.Amount);
        report.NetIncome = report.TotalRevenue - report.TotalExpenses;
        report.GrossProfit = report.TotalRevenue; // Simplified - should exclude COGS
        report.OperatingIncome = report.NetIncome;

        return report;
    }
}

/// <summary>
/// Handler for Cash Flow report
/// </summary>
public class GetCashFlowQueryHandler : IRequestHandler<GetCashFlowQuery, CashFlowDto>
{
    private readonly IDbContext _context;
    private readonly IReportPeriodService _periodService;

    public GetCashFlowQueryHandler(IDbContext context, IReportPeriodService periodService)
    {
        _context = context;
        _periodService = periodService;
    }

    public async Task<CashFlowDto> Handle(GetCashFlowQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = _periodService.GetDateRange(request.Period, request.StartDate, request.EndDate);

        // Simplified cash flow - in production, this would be more complex
        var report = new CashFlowDto
        {
            StartDate = startDate,
            EndDate = endDate,
            OperatingActivities = new List<CashFlowLineDto>
            {
                new CashFlowLineDto { Description = "Net Income", Amount = 0 },
                new CashFlowLineDto { Description = "Adjustments for non-cash items", Amount = 0 }
            },
            InvestingActivities = new List<CashFlowLineDto>(),
            FinancingActivities = new List<CashFlowLineDto>(),
            BeginningCashBalance = 0,
            EndingCashBalance = 0
        };

        return await Task.FromResult(report);
    }
}

/// <summary>
/// Handler for AR Aging report
/// </summary>
public class GetARAgingQueryHandler : IRequestHandler<GetARAgingQuery, ARAgingDto>
{
    private readonly IDbContext _context;

    public GetARAgingQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<ARAgingDto> Handle(GetARAgingQuery request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate ?? DateTime.UtcNow;

        var invoices = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Payments)
            .Where(i => i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.Posted ||
                       i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.PartiallyPaid)
            .ToListAsync(cancellationToken);

        var report = new ARAgingDto
        {
            AsOfDate = asOfDate,
            Lines = invoices.Select(i =>
            {
                var paidAmount = i.Payments?.Sum(p => p.Amount) ?? 0;
                var balanceDue = i.TotalAmount - paidAmount;
                var daysOutstanding = (asOfDate - i.InvoiceDate).Days;

                var line = new ARAgingLineDto
                {
                    InvoiceId = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    ClientId = i.ClientId,
                    ClientName = i.Client?.Name ?? "",
                    InvoiceDate = i.InvoiceDate,
                    DueDate = i.DueDate,
                    DaysOutstanding = daysOutstanding,
                    TotalAmount = i.TotalAmount,
                    PaidAmount = paidAmount,
                    BalanceDue = balanceDue
                };

                // Age the balance
                if (daysOutstanding <= 0)
                    line.Current = balanceDue;
                else if (daysOutstanding <= 30)
                    line.Days1To30 = balanceDue;
                else if (daysOutstanding <= 60)
                    line.Days31To60 = balanceDue;
                else if (daysOutstanding <= 90)
                    line.Days61To90 = balanceDue;
                else
                    line.Over90Days = balanceDue;

                return line;
            })
            .Where(l => l.BalanceDue > 0)
            .OrderBy(l => l.ClientName)
            .ThenBy(l => l.InvoiceDate)
            .ToList()
        };

        report.TotalCurrent = report.Lines.Sum(l => l.Current);
        report.Total1To30Days = report.Lines.Sum(l => l.Days1To30);
        report.Total31To60Days = report.Lines.Sum(l => l.Days31To60);
        report.Total61To90Days = report.Lines.Sum(l => l.Days61To90);
        report.TotalOver90Days = report.Lines.Sum(l => l.Over90Days);
        report.TotalAR = report.Lines.Sum(l => l.BalanceDue);

        return report;
    }
}
