namespace ERP.Domain.WF.Enums;

/// <summary>
/// Status of a workflow step instance
/// </summary>
public enum StepInstanceStatus : byte
{
    /// <summary>
    /// Step is waiting to be activated
    /// </summary>
    Waiting = 1,

    /// <summary>
    /// Step is active and awaiting action
    /// </summary>
    Active = 2,

    /// <summary>
    /// Step was completed (approved)
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Step was rejected
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Step was skipped (conditional logic)
    /// </summary>
    Skipped = 5,

    /// <summary>
    /// Step timed out
    /// </summary>
    TimedOut = 6
}
