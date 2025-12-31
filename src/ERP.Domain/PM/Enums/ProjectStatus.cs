namespace ERP.Domain.PM.Enums;

/// <summary>
/// Project status.
/// </summary>
public enum ProjectStatus : byte
{
    /// <summary>
    /// Project is in planning phase.
    /// </summary>
    Planning = 1,

    /// <summary>
    /// Project is active.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Project is on hold.
    /// </summary>
    OnHold = 3,

    /// <summary>
    /// Project is completed.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Project is cancelled.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Project is closed (invoiced and finalized).
    /// </summary>
    Closed = 6
}
