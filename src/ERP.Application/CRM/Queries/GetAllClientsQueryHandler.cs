using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.DTOs;
using ERP.Domain.CRM.Repositories;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Handler for GetAllClientsQuery.
/// </summary>
public class GetAllClientsQueryHandler : IQueryHandler<GetAllClientsQuery, IEnumerable<ClientDto>>
{
    private readonly IClientRepository _clientRepository;

    public GetAllClientsQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<ClientDto>> Handle(GetAllClientsQuery query, CancellationToken cancellationToken = default)
    {
        var clients = query.ActiveOnly
            ? await _clientRepository.GetActiveClientsAsync(cancellationToken)
            : await _clientRepository.GetAllAsync(cancellationToken);

        return clients.Select(c => new ClientDto
        {
            Id = c.Id,
            ClientNumber = c.ClientNumber,
            ClientType = c.ClientType.ToString(),
            Status = c.Status.ToString(),
            Name = c.Name,
            LegalName = c.LegalName,
            Industry = c.Industry,
            PrimaryEmail = c.PrimaryEmail?.Value,
            PrimaryPhone = c.PrimaryPhone,
            LastContactDate = c.LastContactDate,
            IsActive = c.IsActive,
            CreatedDate = c.CreatedDate
        });
    }
}
