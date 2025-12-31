namespace ERP.Domain.PM.Enums;

/// <summary>
/// Types of projects.
/// </summary>
public enum ProjectType : byte
{
    /// <summary>
    /// Billable client project.
    /// </summary>
    Billable = 1,

    /// <summary>
    /// Internal overhead project.
    /// </summary>
    Overhead = 2,

    /// <summary>
    /// Proposal/pre-sales project.
    /// </summary>
    Proposal = 3,

    /// <summary>
    /// Research and development.
    /// </summary>
    RAndD = 4,

    /// <summary>
    /// Internal product development.
    /// </summary>
    Internal = 5
}
