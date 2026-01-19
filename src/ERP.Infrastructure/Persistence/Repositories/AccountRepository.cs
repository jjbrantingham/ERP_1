using ERP.Domain.Common;
using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using ERP.Domain.FIN.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Account?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByAccountNumberAsync(
        AccountNumber accountNumber,
        CancellationToken cancellationToken = default)
    {
        var accountNumberValue = accountNumber.Value;
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber.Value == accountNumberValue, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .OrderBy(a => a.AccountNumber.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByTypeAsync(
        AccountType type,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.Type == type)
            .OrderBy(a => a.AccountNumber.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.Status == AccountStatus.Active)
            .OrderBy(a => a.AccountNumber.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByParentAccountIdAsync(
        long? parentAccountId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.ParentAccountId == parentAccountId)
            .OrderBy(a => a.AccountNumber.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAccountsForPostingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.Status == AccountStatus.Active && a.AllowPosting)
            .OrderBy(a => a.AccountNumber.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default)
    {
        var accountNumberValue = accountNumber.Value;
        return await _context.Accounts
            .AnyAsync(a => a.AccountNumber.Value == accountNumberValue, cancellationToken);
    }
}
