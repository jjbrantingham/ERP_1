namespace ERP.Domain.FIN.Enums;

/// <summary>
/// Type of journal entry
/// </summary>
public enum JournalEntryType : byte
{
    /// <summary>
    /// General journal entry
    /// </summary>
    General = 1,

    /// <summary>
    /// Adjusting entry (accruals, deferrals, etc.)
    /// </summary>
    Adjusting = 2,

    /// <summary>
    /// Closing entry (period close)
    /// </summary>
    Closing = 3,

    /// <summary>
    /// Reversing entry (reverses a previous entry)
    /// </summary>
    Reversing = 4
}
