namespace ERP.Domain.FIN.Enums;

/// <summary>
/// Status of a journal entry
/// </summary>
public enum JournalEntryStatus : byte
{
    /// <summary>
    /// Draft - can be edited
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Posted - applied to general ledger, immutable
    /// </summary>
    Posted = 2,

    /// <summary>
    /// Voided - reversed, immutable
    /// </summary>
    Voided = 3
}
