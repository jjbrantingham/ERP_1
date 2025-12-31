namespace ERP.Domain.CRM.Enums;

/// <summary>
/// Client relationship status.
/// </summary>
public enum ClientStatus : byte
{
    /// <summary>
    /// Prospective client (lead).
    /// </summary>
    Prospect = 1,

    /// <summary>
    /// Active client with ongoing business.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Inactive client (no recent activity).
    /// </summary>
    Inactive = 3,

    /// <summary>
    /// On hold (temporary suspension).
    /// </summary>
    OnHold = 4,

    /// <summary>
    /// Former client (no longer doing business).
    /// </summary>
    Former = 5
}
