using ERP.Application.Common.Interfaces;
using ERP.Domain.HR.Enums;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Command to create a new employee.
/// </summary>
public class CreateEmployeeCommand : ICommand<long>
{
    public long ResourceTypeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public DateTime HireDate { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public long? ManagerId { get; set; }
    public decimal? BaseSalaryAmount { get; set; }
    public string? BaseSalaryCurrency { get; set; }
    public decimal StandardHoursPerWeek { get; set; } = 40;
    public long? UserId { get; set; }
    public string? Notes { get; set; }
}
