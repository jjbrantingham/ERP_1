using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Repositories;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Handler for CreateRateCommand.
/// </summary>
public class CreateRateCommandHandler : ICommandHandler<CreateRateCommand, long>
{
    private readonly IRateRepository _rateRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IResourceTypeRepository _resourceTypeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateRateCommandHandler(
        IRateRepository rateRepository,
        IEmployeeRepository employeeRepository,
        IResourceTypeRepository resourceTypeRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _rateRepository = rateRepository;
        _employeeRepository = employeeRepository;
        _resourceTypeRepository = resourceTypeRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateRateCommand command, CancellationToken cancellationToken = default)
    {
        // Validate that either EmployeeId or ResourceTypeId is provided (but not both)
        if (!command.EmployeeId.HasValue && !command.ResourceTypeId.HasValue)
            throw new ValidationException("Either EmployeeId or ResourceTypeId must be provided");

        if (command.EmployeeId.HasValue && command.ResourceTypeId.HasValue)
            throw new ValidationException("Cannot specify both EmployeeId and ResourceTypeId");

        // Create Money value objects
        var costRate = new Money(command.CostRateAmount, command.CostRateCurrency);
        var billingRate = new Money(command.BillingRateAmount, command.BillingRateCurrency);

        Rate rate;

        if (command.EmployeeId.HasValue)
        {
            // Verify employee exists
            var employee = await _employeeRepository.GetByIdAsync(command.EmployeeId.Value, cancellationToken);
            if (employee == null)
                throw new NotFoundException("Employee not found");

            // Create rate for employee
            rate = Rate.CreateForEmployee(
                _currentTenant.TenantId,
                command.EmployeeId.Value,
                command.RateType,
                costRate,
                billingRate,
                command.EffectiveDate,
                command.EndDate,
                command.Notes
            );
        }
        else
        {
            // Verify resource type exists
            var resourceType = await _resourceTypeRepository.GetByIdAsync(command.ResourceTypeId!.Value, cancellationToken);
            if (resourceType == null)
                throw new NotFoundException("Resource type not found");

            // Create rate for resource type
            rate = Rate.CreateForResourceType(
                _currentTenant.TenantId,
                command.ResourceTypeId.Value,
                command.RateType,
                costRate,
                billingRate,
                command.EffectiveDate,
                command.EndDate,
                command.Notes
            );
        }

        await _rateRepository.AddAsync(rate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return rate.Id;
    }
}
