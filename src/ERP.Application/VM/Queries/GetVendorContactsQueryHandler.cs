using ERP.Application.Common.Interfaces;
using ERP.Application.VM.DTOs;
using ERP.Domain.VM.Repositories;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Handler for GetVendorContactsQuery.
/// </summary>
public class GetVendorContactsQueryHandler : IQueryHandler<GetVendorContactsQuery, IEnumerable<VendorContactDto>>
{
    private readonly IVendorContactRepository _vendorContactRepository;

    public GetVendorContactsQueryHandler(IVendorContactRepository vendorContactRepository)
    {
        _vendorContactRepository = vendorContactRepository;
    }

    public async Task<IEnumerable<VendorContactDto>> Handle(GetVendorContactsQuery query, CancellationToken cancellationToken = default)
    {
        var contacts = await _vendorContactRepository.GetByVendorIdAsync(query.VendorId, cancellationToken);

        return contacts.Select(c => new VendorContactDto
        {
            Id = c.Id,
            TenantId = c.TenantId,
            VendorId = c.VendorId,
            ContactType = c.ContactType.ToString(),
            FirstName = c.FirstName,
            LastName = c.LastName,
            FullName = c.FullName,
            Title = c.Title,
            Email = c.Email.Value,
            Phone = c.Phone,
            Mobile = c.Mobile,
            IsPrimary = c.IsPrimary,
            Notes = c.Notes,
            IsActive = c.IsActive,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate
        });
    }
}
