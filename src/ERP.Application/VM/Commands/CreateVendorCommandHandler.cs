using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Repositories;
using ERP.Domain.VM.ValueObjects;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Handler for CreateVendorCommand.
/// </summary>
public class CreateVendorCommandHandler : ICommandHandler<CreateVendorCommand, long>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateVendorCommandHandler(
        IVendorRepository vendorRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateVendorCommand command, CancellationToken cancellationToken = default)
    {
        // Generate vendor number
        var vendorNumber = VendorNumber.Generate();

        // Ensure uniqueness
        while (await _vendorRepository.ExistsAsync(vendorNumber, cancellationToken))
        {
            vendorNumber = VendorNumber.Generate();
        }

        // Create email if provided
        Email? email = null;
        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            email = Email.Create(command.Email);
        }

        // Create vendor
        var vendor = Vendor.Create(
            _currentTenant.TenantId,
            vendorNumber,
            command.Name,
            command.VendorType,
            command.TaxId,
            command.Website,
            email,
            command.Phone,
            command.Notes
        );

        await _vendorRepository.AddAsync(vendor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return vendor.Id;
    }
}
