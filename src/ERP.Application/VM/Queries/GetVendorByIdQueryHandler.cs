using ERP.Application.Common.Interfaces;
using ERP.Application.VM.DTOs;
using ERP.Domain.VM.Repositories;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Handler for GetVendorByIdQuery.
/// </summary>
public class GetVendorByIdQueryHandler : IQueryHandler<GetVendorByIdQuery, VendorDto>
{
    private readonly IVendorRepository _vendorRepository;

    public GetVendorByIdQueryHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<VendorDto> Handle(GetVendorByIdQuery query, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetByIdAsync(query.VendorId, cancellationToken);

        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID {query.VendorId} not found.");

        return new VendorDto
        {
            Id = vendor.Id,
            TenantId = vendor.TenantId,
            VendorNumber = vendor.VendorNumber.Value,
            Name = vendor.Name,
            VendorType = vendor.VendorType.ToString(),
            Status = vendor.Status.ToString(),
            TaxId = vendor.TaxId,
            Website = vendor.Website,
            Email = vendor.Email?.Value,
            Phone = vendor.Phone,
            Fax = vendor.Fax,
            AddressStreet = vendor.AddressStreet,
            AddressStreet2 = vendor.AddressStreet2,
            AddressCity = vendor.AddressCity,
            AddressStateProvince = vendor.AddressStateProvince,
            AddressPostalCode = vendor.AddressPostalCode,
            AddressCountry = vendor.AddressCountry,
            PaymentTerms = vendor.PaymentTerms,
            PaymentDueDays = vendor.PaymentDueDays,
            PreferredPaymentMethod = vendor.PreferredPaymentMethod,
            AccountNumber = vendor.AccountNumber,
            CreditLimit = vendor.CreditLimit,
            Notes = vendor.Notes,
            IsActive = vendor.IsActive,
            CreatedDate = vendor.CreatedDate,
            ModifiedDate = vendor.ModifiedDate
        };
    }
}
