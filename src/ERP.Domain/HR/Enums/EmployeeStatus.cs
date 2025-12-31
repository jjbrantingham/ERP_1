namespace ERP.Domain.HR.Enums;

/// <summary>
/// Employee status in the organization.
/// </summary>
public enum EmployeeStatus : byte
{
    /// <summary>
    /// Employee is active and working.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Employee is on leave (vacation, sick, etc.).
    /// </summary>
    OnLeave = 2,

    /// <summary>
    /// Employee has been terminated.
    /// </summary>
    Terminated = 3,

    /// <summary>
    /// Employee has resigned.
    /// </summary>
    Resigned = 4,

    /// <summary>
    /// Employee is retired.
    /// </summary>
    Retired = 5,

    /// <summary>
    /// Employee is suspended.
    /// </summary>
    Suspended = 6
}
