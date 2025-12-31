using ERP.Application.Common.Interfaces;
using ERP.Domain.HR.Enums;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Command to update an employee's information.
/// </summary>
public class UpdateEmployeeCommand : ICommand
{
    public long EmployeeId { get; set; }
    public long ResourceTypeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public long? ManagerId { get; set; }
    public decimal? BaseSalaryAmount { get; set; }
    public string? BaseSalaryCurrency { get; set; }
    public decimal StandardHoursPerWeek { get; set; } = 40;
    public string? Notes { get; set; }
}
