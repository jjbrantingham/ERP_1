using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.CRM.DTOs;
using ERP.Domain.CRM.Repositories;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Handler for GetClientContactsQuery.
/// </summary>
public class GetClientContactsQueryHandler : IQueryHandler<GetClientContactsQuery, IEnumerable<ContactDto>>
{
    private readonly IContactRepository _contactRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetClientContactsQueryHandler(IContactRepository contactRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _contactRepository = contactRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<ContactDto>> Handle(GetClientContactsQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var contacts = await _contactRepository.GetByClientIdAsync(query.ClientId, cancellationToken);

        return contacts.Select(c => new ContactDto
        {
            Id = c.Id,
            ClientId = c.ClientId,
            ContactType = c.ContactType.ToString(),
            FirstName = c.FirstName,
            LastName = c.LastName,
            FullName = c.FullName,
            MiddleName = c.MiddleName,
            Email = c.Email.Value,
            PhoneNumber = c.PhoneNumber,
            MobileNumber = c.MobileNumber,
            JobTitle = c.JobTitle,
            Department = c.Department,
            IsPrimary = c.IsPrimary,
            IsActive = c.IsActive,
            Notes = c.Notes,
            CreatedDate = c.CreatedDate
        });
    }
}
