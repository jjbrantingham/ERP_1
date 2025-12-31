namespace ERP.Domain.CRM.Enums;

/// <summary>
/// Types of contacts.
/// </summary>
public enum ContactType : byte
{
    /// <summary>
    /// Primary contact for the client.
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Billing and financial contact.
    /// </summary>
    Billing = 2,

    /// <summary>
    /// Technical contact.
    /// </summary>
    Technical = 3,

    /// <summary>
    /// Sales contact.
    /// </summary>
    Sales = 4,

    /// <summary>
    /// Executive/decision maker.
    /// </summary>
    Executive = 5,

    /// <summary>
    /// General contact.
    /// </summary>
    General = 6
}
