using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Repositories;
using ERP.Domain.HR.ValueObjects;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Handler for CreateEmployeeCommand.
/// </summary>
public class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, long>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IResourceTypeRepository _resourceTypeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        IResourceTypeRepository resourceTypeRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)
    {
        _employeeRepository = employeeRepository;
        _resourceTypeRepository = resourceTypeRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<long> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Verify resource type exists
        var resourceType = await _resourceTypeRepository.GetByIdAsync(command.ResourceTypeId, cancellationToken);
        if (resourceType == null)
            throw new NotFoundException("Resource type not found");

        // Verify manager exists if specified
        if (command.ManagerId.HasValue)
        {
            var manager = await _employeeRepository.GetByIdAsync(command.ManagerId.Value, cancellationToken);
            if (manager == null)
                throw new NotFoundException("Manager not found");
        }

        // Check if email already exists
        var existingEmployee = await _employeeRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingEmployee != null)
            throw new ValidationException("Email", "Email already in use");

        // Generate employee number
        var employeeNumber = EmployeeNumber.Generate();

        // Create base salary if provided
        Money? baseSalary = null;
        if (command.BaseSalaryAmount.HasValue && !string.IsNullOrWhiteSpace(command.BaseSalaryCurrency))
        {
            baseSalary = new Money(command.BaseSalaryAmount.Value, command.BaseSalaryCurrency);
        }

        // Create employee
        var employee = Employee.Create(
            _currentTenant.TenantId,
            employeeNumber,
            command.ResourceTypeId,
            command.FirstName,
            command.LastName,
            new Email(command.Email),
            command.EmploymentType,
            command.HireDate,
            command.MiddleName,
            command.PhoneNumber,
            command.MobileNumber,
            command.DateOfBirth,
            command.JobTitle,
            command.Department,
            command.ManagerId,
            baseSalary,
            command.StandardHoursPerWeek,
            command.UserId,
            command.Notes
        );

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
