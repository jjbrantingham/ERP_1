namespace ERP.Domain.VM.Enums;

/// <summary>
/// Type of vendor contact.
/// </summary>
public enum ContactType : byte
{
    /// <summary>
    /// Primary contact for the vendor.
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Billing and invoicing contact.
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
    /// Support contact.
    /// </summary>
    Support = 5,

    /// <summary>
    /// Other contact type.
    /// </summary>
    Other = 99
}
