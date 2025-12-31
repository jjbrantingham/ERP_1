using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.HR.DTOs;
using ERP.Domain.HR.Repositories;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Handler for GetEmployeeByIdQuery.
/// </summary>
public class GetEmployeeByIdQueryHandler : IQueryHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery query, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdWithRatesAsync(query.EmployeeId, cancellationToken);
        if (employee == null)
            throw new NotFoundException("Employee not found");

        return new EmployeeDto
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
            ManagerName = employee.Manager != null ? employee.Manager.FullName : null,
            BaseSalaryAmount = employee.BaseSalary?.Amount,
            BaseSalaryCurrency = employee.BaseSalary?.Currency,
            StandardHoursPerWeek = employee.StandardHoursPerWeek,
            IsAvailableForProjects = employee.IsAvailableForProjects,
            Notes = employee.Notes,
            CreatedDate = employee.CreatedDate,
            ModifiedDate = employee.ModifiedDate
        };
    }
}
