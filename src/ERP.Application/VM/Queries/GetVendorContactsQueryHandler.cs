using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.VM.DTOs;
using ERP.Domain.VM.Repositories;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Handler for GetVendorContactsQuery.
/// </summary>
public class GetVendorContactsQueryHandler : IQueryHandler<GetVendorContactsQuery, IEnumerable<VendorContactDto>>
{
    private readonly IVendorContactRepository _vendorContactRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetVendorContactsQueryHandler(IVendorContactRepository vendorContactRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _vendorContactRepository = vendorContactRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<VendorContactDto>> Handle(GetVendorContactsQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

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
