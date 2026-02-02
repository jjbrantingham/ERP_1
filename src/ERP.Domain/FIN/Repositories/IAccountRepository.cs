using ERP.Domain.Common;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.ValueObjects;

namespace ERP.Domain.FIN.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    new Task<Account?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Account?> GetByAccountNumberAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default);
    new Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetByTypeAsync(AccountType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetByParentAccountIdAsync(long? parentAccountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetAccountsForPostingAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default);
}
