using ERP.Application.Common.Interfaces;
using ERP.Domain.HR.Enums;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Command to update an employee's information.
/// </summary>
public class UpdateEmployeeCommand : ICommand
{
    public long EmployeeId { get; init; }
    public long ResourceTypeId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? MobileNumber { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public EmploymentType EmploymentType { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public long? ManagerId { get; init; }
    public decimal? BaseSalaryAmount { get; init; }
    public string? BaseSalaryCurrency { get; init; }
    public decimal StandardHoursPerWeek { get; init; } = 40;
    public string? Notes { get; init; }
}
