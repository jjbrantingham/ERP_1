namespace ERP.Domain.FIN.Enums;

/// <summary>
/// Types of accounts in the chart of accounts
/// </summary>
public enum AccountType : byte
{
    /// <summary>
    /// Assets - Resources owned by the company (Debit increases, Credit decreases)
    /// </summary>
    Asset = 1,

    /// <summary>
    /// Liabilities - Obligations owed to others (Credit increases, Debit decreases)
    /// </summary>
    Liability = 2,

    /// <summary>
    /// Equity - Owner's stake in the company (Credit increases, Debit decreases)
    /// </summary>
    Equity = 3,

    /// <summary>
    /// Revenue - Income from operations (Credit increases, Debit decreases)
    /// </summary>
    Revenue = 4,

    /// <summary>
    /// Expense - Costs of operations (Debit increases, Credit decreases)
    /// </summary>
    Expense = 5
}
