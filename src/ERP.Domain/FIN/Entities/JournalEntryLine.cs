using ERP.Domain.Common.Entities;

namespace ERP.Domain.FIN.Entities;

/// <summary>
/// Represents a single line in a journal entry
/// </summary>
public class JournalEntryLine : Entity
{
    public long Id { get; private set; }
    public long JournalEntryId { get; private set; }
    public long AccountId { get; private set; }
    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }
    public string Description { get; private set; }

    private JournalEntryLine()
    {
        Description = null!;
    }

    /// <summary>
    /// Create a journal entry line
    /// </summary>
    public static JournalEntryLine Create(
        long accountId,
        decimal debitAmount,
        decimal creditAmount,
        string description)
    {
        if (accountId <= 0)
            throw new ArgumentException("Account ID must be greater than zero", nameof(accountId));

        if (debitAmount < 0 || creditAmount < 0)
            throw new ArgumentException("Debit and credit amounts must be non-negative");

        if (debitAmount > 0 && creditAmount > 0)
            throw new ArgumentException("A line cannot have both debit and credit amounts");

        if (debitAmount == 0 && creditAmount == 0)
            throw new ArgumentException("A line must have either a debit or credit amount");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        // Round amounts to 2 decimal places
        debitAmount = Math.Round(debitAmount, 2, MidpointRounding.AwayFromZero);
        creditAmount = Math.Round(creditAmount, 2, MidpointRounding.AwayFromZero);

        return new JournalEntryLine
        {
            AccountId = accountId,
            DebitAmount = debitAmount,
            CreditAmount = creditAmount,
            Description = description
        };
    }

    /// <summary>
    /// Get the net amount (debit - credit)
    /// </summary>
    public decimal GetNetAmount()
    {
        return DebitAmount - CreditAmount;
    }
}
