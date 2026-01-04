using ERP.Application.RPT.DTOs;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using ERP.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.RPT.Queries;

public class GetIncomeStatementQueryHandler : IRequestHandler<GetIncomeStatementQuery, IncomeStatementDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly ERPDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetIncomeStatementQueryHandler(
        IAccountRepository accountRepository,
        IJournalEntryRepository journalEntryRepository,
        ERPDbContext context,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _context = context;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IncomeStatementDto> Handle(GetIncomeStatementQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Get all accounts
        var accounts = await _accountRepository.GetAllAsync(cancellationToken);

        // Get journal entries for the period
        var journalEntries = await _journalEntryRepository.GetByDateRangeAsync(
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var incomeStatement = new IncomeStatementDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Currency = request.Currency
        };

        // Calculate account balances for the period
        var accountBalances = new Dictionary<long, decimal>();

        foreach (var entry in journalEntries.Where(e => e.Status == ERP.Domain.FIN.Enums.JournalEntryStatus.Posted))
        {
            foreach (var line in entry.Lines)
            {
                if (!accountBalances.ContainsKey(line.AccountId))
                    accountBalances[line.AccountId] = 0m;

                var account = accounts.FirstOrDefault(a => a.Id == line.AccountId);
                if (account != null)
                {
                    // Revenue and Expense accounts: Credit increases Revenue, Debit increases Expense
                    if (account.Type == AccountType.Revenue)
                        accountBalances[line.AccountId] += line.CreditAmount - line.DebitAmount;
                    else if (account.Type == AccountType.Expense)
                        accountBalances[line.AccountId] += line.DebitAmount - line.CreditAmount;
                }
            }
        }

        // Revenue
        var revenueAccounts = accounts.Where(a => a.Type == AccountType.Revenue).ToList();
        incomeStatement.RevenueAccounts = revenueAccounts
            .Select(a => new AccountLineDto
            {
                AccountNumber = a.AccountNumber.Value,
                AccountName = a.Name,
                Balance = accountBalances.ContainsKey(a.Id) ? accountBalances[a.Id] : 0m
            }).ToList();

        incomeStatement.TotalRevenue = incomeStatement.RevenueAccounts.Sum(a => a.Balance);

        // Expenses
        var expenseAccounts = accounts.Where(a => a.Type == AccountType.Expense).ToList();
        incomeStatement.OperatingExpenses = expenseAccounts
            .Select(a => new AccountLineDto
            {
                AccountNumber = a.AccountNumber.Value,
                AccountName = a.Name,
                Balance = accountBalances.ContainsKey(a.Id) ? accountBalances[a.Id] : 0m
            }).ToList();

        incomeStatement.TotalOperatingExpenses = incomeStatement.OperatingExpenses.Sum(a => a.Balance);

        // Calculations
        incomeStatement.GrossProfit = incomeStatement.TotalRevenue - incomeStatement.TotalCostOfGoodsSold;
        incomeStatement.GrossProfitMargin = incomeStatement.TotalRevenue > 0
            ? incomeStatement.GrossProfit / incomeStatement.TotalRevenue * 100
            : 0m;

        incomeStatement.OperatingIncome = incomeStatement.GrossProfit - incomeStatement.TotalOperatingExpenses;
        incomeStatement.OperatingMargin = incomeStatement.TotalRevenue > 0
            ? incomeStatement.OperatingIncome / incomeStatement.TotalRevenue * 100
            : 0m;

        incomeStatement.NetIncome = incomeStatement.OperatingIncome + incomeStatement.TotalOtherIncome - incomeStatement.TotalOtherExpenses;
        incomeStatement.NetProfitMargin = incomeStatement.TotalRevenue > 0
            ? incomeStatement.NetIncome / incomeStatement.TotalRevenue * 100
            : 0m;

        return incomeStatement;
    }
}
