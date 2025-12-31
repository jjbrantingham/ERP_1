namespace ERP.Domain.HR.Enums;

/// <summary>
/// Types of employment relationships.
/// </summary>
public enum EmploymentType : byte
{
    /// <summary>
    /// Full-time employee.
    /// </summary>
    FullTime = 1,

    /// <summary>
    /// Part-time employee.
    /// </summary>
    PartTime = 2,

    /// <summary>
    /// Contract employee (temporary).
    /// </summary>
    Contract = 3,

    /// <summary>
    /// Consultant (external).
    /// </summary>
    Consultant = 4,

    /// <summary>
    /// Intern.
    /// </summary>
    Intern = 5
}
