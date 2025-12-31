using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.DTOs;
using ERP.Domain.CRM.Repositories;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Handler for GetClientContactsQuery.
/// </summary>
public class GetClientContactsQueryHandler : IQueryHandler<GetClientContactsQuery, IEnumerable<ContactDto>>
{
    private readonly IContactRepository _contactRepository;

    public GetClientContactsQueryHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<IEnumerable<ContactDto>> Handle(GetClientContactsQuery query, CancellationToken cancellationToken = default)
    {
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
