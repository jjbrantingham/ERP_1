using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Repositories;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Handler for CreateResourceTypeCommand.
/// </summary>
public class CreateResourceTypeCommandHandler : ICommandHandler<CreateResourceTypeCommand, long>
{
    private readonly IResourceTypeRepository _resourceTypeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateResourceTypeCommandHandler(
        IResourceTypeRepository resourceTypeRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _resourceTypeRepository = resourceTypeRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateResourceTypeCommand command, CancellationToken cancellationToken = default)
    {
        // Check if name already exists
        var existingResourceType = await _resourceTypeRepository.GetByNameAsync(command.Name, cancellationToken);
        if (existingResourceType != null)
            throw new ValidationException("Resource type name already exists");

        // Check if code already exists (if provided)
        if (!string.IsNullOrWhiteSpace(command.Code))
        {
            var existingCode = await _resourceTypeRepository.GetByCodeAsync(command.Code, cancellationToken);
            if (existingCode != null)
                throw new ValidationException("Resource type code already exists");
        }

        // Create resource type
        var resourceType = ResourceType.Create(
            _currentTenant.TenantId,
            command.Name,
            command.Description,
            command.Code,
            command.DisplayOrder
        );

        await _resourceTypeRepository.AddAsync(resourceType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return resourceType.Id;
    }
}
