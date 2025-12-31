using ERP.Application.Common.Interfaces;
using ERP.Application.VM.DTOs;
using ERP.Domain.VM.Repositories;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Handler for GetAllVendorsQuery.
/// </summary>
public class GetAllVendorsQueryHandler : IQueryHandler<GetAllVendorsQuery, IEnumerable<VendorDto>>
{
    private readonly IVendorRepository _vendorRepository;

    public GetAllVendorsQueryHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<IEnumerable<VendorDto>> Handle(GetAllVendorsQuery query, CancellationToken cancellationToken = default)
    {
        var vendors = query.ActiveOnly
            ? await _vendorRepository.GetActiveVendorsAsync(cancellationToken)
            : await _vendorRepository.GetAllAsync(cancellationToken);

        return vendors.Select(v => new VendorDto
        {
            Id = v.Id,
            TenantId = v.TenantId,
            VendorNumber = v.VendorNumber.Value,
            Name = v.Name,
            VendorType = v.VendorType.ToString(),
            Status = v.Status.ToString(),
            TaxId = v.TaxId,
            Website = v.Website,
            Email = v.Email?.Value,
            Phone = v.Phone,
            Fax = v.Fax,
            AddressStreet = v.AddressStreet,
            AddressStreet2 = v.AddressStreet2,
            AddressCity = v.AddressCity,
            AddressStateProvince = v.AddressStateProvince,
            AddressPostalCode = v.AddressPostalCode,
            AddressCountry = v.AddressCountry,
            PaymentTerms = v.PaymentTerms,
            PaymentDueDays = v.PaymentDueDays,
            PreferredPaymentMethod = v.PreferredPaymentMethod,
            AccountNumber = v.AccountNumber,
            CreditLimit = v.CreditLimit,
            Notes = v.Notes,
            IsActive = v.IsActive,
            CreatedDate = v.CreatedDate,
            ModifiedDate = v.ModifiedDate
        });
    }
}
