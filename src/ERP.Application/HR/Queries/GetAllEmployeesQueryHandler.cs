using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.HR.DTOs;
using ERP.Domain.HR.Repositories;
using MediatR;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Handler for GetAllEmployeesQuery.
/// </summary>
public class GetAllEmployeesQueryHandler : IRequestHandler<GetAllEmployeesQuery, IEnumerable<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _employeeRepository = employeeRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<EmployeeDto>> Handle(GetAllEmployeesQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

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
