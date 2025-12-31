namespace ERP.Domain.CRM.Enums;

/// <summary>
/// Types of clients.
/// </summary>
public enum ClientType : byte
{
    /// <summary>
    /// Corporate/business client.
    /// </summary>
    Corporate = 1,

    /// <summary>
    /// Individual/personal client.
    /// </summary>
    Individual = 2,

    /// <summary>
    /// Government entity.
    /// </summary>
    Government = 3,

    /// <summary>
    /// Non-profit organization.
    /// </summary>
    NonProfit = 4,

    /// <summary>
    /// Partner or reseller.
    /// </summary>
    Partner = 5
}
