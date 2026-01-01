namespace ERP.Domain.FIN.Enums;

/// <summary>
/// Status of an account
/// </summary>
public enum AccountStatus : byte
{
    /// <summary>
    /// Account is active and can be used in transactions
    /// </summary>
    Active = 1,

    /// <summary>
    /// Account is inactive and cannot be used in new transactions
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Account is closed and cannot be used
    /// </summary>
    Closed = 3
}
