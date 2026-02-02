using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.CRM.DTOs;
using ERP.Domain.CRM.Repositories;
using MediatR;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Handler for GetAllClientsQuery.
/// </summary>
public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, IEnumerable<ClientDto>>
{
    private readonly IClientRepository _clientRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetAllClientsQueryHandler(IClientRepository clientRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _clientRepository = clientRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<ClientDto>> Handle(GetAllClientsQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

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
