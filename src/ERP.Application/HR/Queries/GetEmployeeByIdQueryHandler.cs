using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.HR.DTOs;
using ERP.Domain.HR.Repositories;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Handler for GetEmployeeByIdQuery.
/// </summary>
public class GetEmployeeByIdQueryHandler : IQueryHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _employeeRepository = employeeRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

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
