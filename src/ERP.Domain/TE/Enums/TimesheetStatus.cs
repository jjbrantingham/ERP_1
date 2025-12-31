namespace ERP.Domain.TE.Enums;

/// <summary>
/// Timesheet approval status.
/// </summary>
public enum TimesheetStatus : byte
{
    /// <summary>
    /// Timesheet is in draft mode.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Timesheet has been submitted for approval.
    /// </summary>
    Submitted = 2,

    /// <summary>
    /// Timesheet has been approved.
    /// </summary>
    Approved = 3,

    /// <summary>
    /// Timesheet has been rejected.
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Timesheet has been recalled by the employee.
    /// </summary>
    Recalled = 5
}
