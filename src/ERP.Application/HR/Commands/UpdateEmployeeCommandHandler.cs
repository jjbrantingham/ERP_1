using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.HR.Repositories;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Handler for UpdateEmployeeCommand.
/// </summary>
public class UpdateEmployeeCommandHandler : ICommandHandler<UpdateEmployeeCommand>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IResourceTypeRepository _resourceTypeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        IResourceTypeRepository resourceTypeRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _employeeRepository = employeeRepository;
        _resourceTypeRepository = resourceTypeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        // Get employee
        var employee = await _employeeRepository.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee == null)
            throw new NotFoundException("Employee not found");

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

        // Check if email is being changed and if new email already exists
        var existingEmail = new Email(employee.Email.Value);
        var newEmail = new Email(command.Email);
        if (existingEmail.Value != newEmail.Value)
        {
            var existingEmployee = await _employeeRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (existingEmployee != null && existingEmployee.Id != command.EmployeeId)
                throw new ValidationException("Email", "Email already in use");
        }

        // Create base salary if provided
        Money? baseSalary = null;
        if (command.BaseSalaryAmount.HasValue && !string.IsNullOrWhiteSpace(command.BaseSalaryCurrency))
        {
            baseSalary = new Money(command.BaseSalaryAmount.Value, command.BaseSalaryCurrency);
        }

        // Update personal info
        employee.UpdatePersonalInfo(
            command.FirstName,
            command.LastName,
            newEmail,
            command.MiddleName,
            command.PhoneNumber,
            command.MobileNumber,
            command.DateOfBirth
        );

        // Update employment info
        employee.UpdateEmploymentInfo(
            command.ResourceTypeId,
            command.EmploymentType,
            command.JobTitle,
            command.Department,
            command.ManagerId,
            baseSalary,
            command.StandardHoursPerWeek
        );

        // Update notes
        employee.UpdateNotes(command.Notes);

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
