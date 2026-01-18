using ERP.Application.RPT.DTOs;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using MediatR;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;

namespace ERP.Application.RPT.Queries;

public class GetBalanceSheetQueryHandler : IRequestHandler<GetBalanceSheetQuery, BalanceSheetDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetBalanceSheetQueryHandler(IAccountRepository accountRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _accountRepository = accountRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<BalanceSheetDto> Handle(GetBalanceSheetQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Get all accounts
        var accounts = await _accountRepository.GetAllAsync(cancellationToken);

        var balanceSheet = new BalanceSheetDto
        {
            AsOfDate = request.AsOfDate,
            Currency = request.Currency
        };

        // Assets
        var assetAccounts = accounts.Where(a => a.Type == AccountType.Asset).ToList();
        balanceSheet.CurrentAssets = assetAccounts
            .Where(a => a.AccountNumber.Value.StartsWith("1") && a.AccountNumber.Value.CompareTo("1200") < 0)
            .Select(a => new AccountLineDto
            {
                AccountNumber = a.AccountNumber.Value,
                AccountName = a.Name,
                Balance = a.Balance
            }).ToList();

        balanceSheet.TotalCurrentAssets = balanceSheet.CurrentAssets.Sum(a => a.Balance);

        balanceSheet.FixedAssets = assetAccounts
            .Where(a => a.AccountNumber.Value.StartsWith("1") && a.AccountNumber.Value.CompareTo("1200") >= 0)
            .Select(a => new AccountLineDto
            {
                AccountNumber = a.AccountNumber.Value,
                AccountName = a.Name,
                Balance = a.Balance
            }).ToList();

        balanceSheet.TotalFixedAssets = balanceSheet.FixedAssets.Sum(a => a.Balance);
        balanceSheet.TotalAssets = balanceSheet.TotalCurrentAssets + balanceSheet.TotalFixedAssets;

        // Liabilities
        var liabilityAccounts = accounts.Where(a => a.Type == AccountType.Liability).ToList();
        balanceSheet.CurrentLiabilities = liabilityAccounts
            .Where(a => a.AccountNumber.Value.StartsWith("2") && a.AccountNumber.Value.CompareTo("2200") < 0)
            .Select(a => new AccountLineDto
            {
                AccountNumber = a.AccountNumber.Value,
                AccountName = a.Name,
                Balance = a.Balance
            }).ToList();

        balanceSheet.TotalCurrentLiabilities = balanceSheet.CurrentLiabilities.Sum(a => a.Balance);

        balanceSheet.LongTermLiabilities = liabilityAccounts
            .Where(a => a.AccountNumber.Value.StartsWith("2") && a.AccountNumber.Value.CompareTo("2200") >= 0)
            .Select(a => new AccountLineDto
            {
                AccountNumber = a.AccountNumber.Value,
                AccountName = a.Name,
                Balance = a.Balance
            }).ToList();

        balanceSheet.TotalLongTermLiabilities = balanceSheet.LongTermLiabilities.Sum(a => a.Balance);
        balanceSheet.TotalLiabilities = balanceSheet.TotalCurrentLiabilities + balanceSheet.TotalLongTermLiabilities;

        // Equity
        var equityAccounts = accounts.Where(a => a.Type == AccountType.Equity).ToList();
        balanceSheet.Equity = equityAccounts
            .Select(a => new AccountLineDto
            {
                AccountNumber = a.AccountNumber.Value,
                AccountName = a.Name,
                Balance = a.Balance
            }).ToList();

        balanceSheet.TotalEquity = balanceSheet.Equity.Sum(a => a.Balance);
        balanceSheet.TotalLiabilitiesAndEquity = balanceSheet.TotalLiabilities + balanceSheet.TotalEquity;

        return balanceSheet;
    }
}
