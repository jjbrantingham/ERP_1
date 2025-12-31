using ERP.Application.Common.Interfaces;
using ERP.Application.HR.DTOs;
using ERP.Domain.HR.Repositories;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Handler for GetAllEmployeesQuery.
/// </summary>
public class GetAllEmployeesQueryHandler : IQueryHandler<GetAllEmployeesQuery, IEnumerable<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<IEnumerable<EmployeeDto>> Handle(GetAllEmployeesQuery query, CancellationToken cancellationToken = default)
    {
        var employees = query.ActiveOnly
            ? await _employeeRepository.GetActiveEmployeesAsync(cancellationToken)
            : await _employeeRepository.GetAllAsync(cancellationToken);

        return employees.Select(employee => new EmployeeDto
        {
            Id = employee.Id,
            EmployeeNumber = employee.EmployeeNumber.Value,
            UserId = employee.UserId,
            ResourceTypeId = employee.ResourceTypeId,
            ResourceTypeName = employee.ResourceType?.Name,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            MiddleName = employee.MiddleName,
            Email = employee.Email.Value,
            PhoneNumber = employee.PhoneNumber,
            MobileNumber = employee.MobileNumber,
            DateOfBirth = employee.DateOfBirth,
            EmploymentType = employee.EmploymentType.ToString(),
            Status = employee.Status.ToString(),
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            JobTitle = employee.JobTitle,
            Department = employee.Department,
            ManagerId = employee.ManagerId,
            BaseSalaryAmount = employee.BaseSalary?.Amount,
            BaseSalaryCurrency = employee.BaseSalary?.Currency,
            StandardHoursPerWeek = employee.StandardHoursPerWeek,
            IsAvailableForProjects = employee.IsAvailableForProjects,
            Notes = employee.Notes,
            CreatedDate = employee.CreatedDate,
            ModifiedDate = employee.ModifiedDate
        });
    }
}
