namespace ERP.Domain.CRM.Enums;

/// <summary>
/// Types of notes.
/// </summary>
public enum NoteType : byte
{
    /// <summary>
    /// General note.
    /// </summary>
    General = 1,

    /// <summary>
    /// Meeting notes.
    /// </summary>
    Meeting = 2,

    /// <summary>
    /// Phone call notes.
    /// </summary>
    PhoneCall = 3,

    /// <summary>
    /// Email correspondence.
    /// </summary>
    Email = 4,

    /// <summary>
    /// Important reminder or action item.
    /// </summary>
    Reminder = 5,

    /// <summary>
    /// Issue or problem report.
    /// </summary>
    Issue = 6,

    /// <summary>
    /// Follow-up note.
    /// </summary>
    FollowUp = 7
}
