using ERP.Domain.Common.Entities;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Events;
using ERP.Domain.FIN.ValueObjects;

namespace ERP.Domain.FIN.Entities;

/// <summary>
/// Represents an account in the chart of accounts
/// </summary>
public class Account : AggregateRoot
{
    public long Id { get; private set; }
    public Guid TenantId { get; private set; }
    public AccountNumber AccountNumber { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public AccountType Type { get; private set; }
    public AccountStatus Status { get; private set; }

    /// <summary>
    /// Parent account for hierarchical structure (null if top-level)
    /// </summary>
    public long? ParentAccountId { get; private set; }

    /// <summary>
    /// Current balance of the account
    /// </summary>
    public decimal Balance { get; private set; }

    /// <summary>
    /// Currency code (ISO 4217)
    /// </summary>
    public string Currency { get; private set; }

    /// <summary>
    /// Whether this account can have transactions posted directly to it
    /// (false for parent/header accounts)
    /// </summary>
    public bool AllowPosting { get; private set; }

    /// <summary>
    /// Whether this account is reconciled with bank/external statements
    /// </summary>
    public bool RequiresReconciliation { get; private set; }

    // Audit fields
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;

    private Account()
    {
        AccountNumber = null!;
        Name = null!;
        Currency = null!;
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    public static Account Create(
        Guid tenantId,
        AccountNumber accountNumber,
        string name,
        AccountType type,
        string currency,
        bool allowPosting = true,
        string? description = null,
        long? parentAccountId = null,
        bool requiresReconciliation = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Account name cannot exceed 200 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be 3-letter ISO code", nameof(currency));

        var account = new Account
        {
            TenantId = tenantId,
            AccountNumber = accountNumber,
            Name = name,
            Description = description,
            Type = type,
            Status = AccountStatus.Active,
            ParentAccountId = parentAccountId,
            Balance = 0m,
            Currency = currency.ToUpperInvariant(),
            AllowPosting = allowPosting,
            RequiresReconciliation = requiresReconciliation,
            CreatedDate = DateTime.UtcNow
        };

        account.AddDomainEvent(new AccountCreatedEvent(account.Id, tenantId, accountNumber.Value, type));

        return account;
    }

    /// <summary>
    /// Update account details
    /// </summary>
    public void Update(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Account name cannot exceed 200 characters", nameof(name));

        Name = name;
        Description = description;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Change account status
    /// </summary>
    public void ChangeStatus(AccountStatus newStatus)
    {
        if (Status == newStatus)
            return;

        var oldStatus = Status;
        Status = newStatus;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new AccountStatusChangedEvent(Id, TenantId, oldStatus, newStatus));
    }

    /// <summary>
    /// Apply a transaction to the account
    /// Updates the balance based on account type and transaction amounts
    /// </summary>
    public void ApplyTransaction(decimal debitAmount, decimal creditAmount)
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException($"Cannot post to {Status} account");

        if (!AllowPosting)
            throw new InvalidOperationException("Cannot post directly to this account (parent/header account)");

        // Asset and Expense accounts: Debit increases, Credit decreases
        // Liability, Equity, Revenue accounts: Credit increases, Debit decreases
        if (Type == AccountType.Asset || Type == AccountType.Expense)
        {
            Balance += debitAmount - creditAmount;
        }
        else if (Type == AccountType.Liability || Type == AccountType.Equity || Type == AccountType.Revenue)
        {
            Balance += creditAmount - debitAmount;
        }

        Balance = Math.Round(Balance, 2, MidpointRounding.AwayFromZero);
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Get account balance as of a specific date
    /// </summary>
    public decimal GetBalance(DateTime? asOfDate = null)
    {
        // Current balance if no date specified
        if (!asOfDate.HasValue)
            return Balance;

        // Historical balance would require querying transactions
        // This would be implemented in the repository/application layer
        throw new NotImplementedException("Historical balance calculation requires transaction history query");
    }

    /// <summary>
    /// Deactivate the account
    /// </summary>
    public void Deactivate()
    {
        if (Status == AccountStatus.Active)
        {
            ChangeStatus(AccountStatus.Inactive);
        }
    }

    /// <summary>
    /// Close the account
    /// </summary>
    public void Close()
    {
        if (Balance != 0)
            throw new InvalidOperationException("Cannot close account with non-zero balance");

        ChangeStatus(AccountStatus.Closed);
    }
}
