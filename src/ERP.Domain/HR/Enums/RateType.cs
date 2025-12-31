namespace ERP.Domain.HR.Enums;

/// <summary>
/// Types of rates for employees and resource types.
/// </summary>
public enum RateType : byte
{
    /// <summary>
    /// Standard hourly rate.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Overtime rate (typically 1.5x or 2x standard).
    /// </summary>
    Overtime = 2,

    /// <summary>
    /// Holiday rate.
    /// </summary>
    Holiday = 3,

    /// <summary>
    /// Weekend rate.
    /// </summary>
    Weekend = 4,

    /// <summary>
    /// On-call rate.
    /// </summary>
    OnCall = 5,

    /// <summary>
    /// Emergency rate.
    /// </summary>
    Emergency = 6
}
