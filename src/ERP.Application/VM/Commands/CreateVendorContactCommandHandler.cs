using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Repositories;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Handler for CreateVendorContactCommand.
/// </summary>
public class CreateVendorContactCommandHandler : ICommandHandler<CreateVendorContactCommand, long>
{
    private readonly IVendorContactRepository _vendorContactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateVendorContactCommandHandler(
        IVendorContactRepository vendorContactRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _vendorContactRepository = vendorContactRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateVendorContactCommand command, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);

        var contact = VendorContact.Create(
            _currentTenant.TenantId,
            command.VendorId,
            command.ContactType,
            command.FirstName,
            command.LastName,
            email,
            command.Title,
            command.Phone,
            command.Mobile,
            command.IsPrimary,
            command.Notes
        );

        await _vendorContactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}
