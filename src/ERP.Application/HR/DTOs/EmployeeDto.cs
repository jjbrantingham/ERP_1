using ERP.Domain.HR.Enums;

namespace ERP.Application.HR.DTOs;

/// <summary>
/// Data transfer object for Employee.
/// </summary>
public class EmployeeDto
{
    public long Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public long? UserId { get; set; }
    public long ResourceTypeId { get; set; }
    public string? ResourceTypeName { get; set; }

    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // Employment Information
    public string EmploymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public long? ManagerId { get; set; }
    public string? ManagerName { get; set; }

    // Compensation
    public decimal? BaseSalaryAmount { get; set; }
    public string? BaseSalaryCurrency { get; set; }

    // Availability
    public decimal StandardHoursPerWeek { get; set; }
    public bool IsAvailableForProjects { get; set; }

    // Additional
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
