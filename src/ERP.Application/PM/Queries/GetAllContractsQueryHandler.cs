using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.PM.DTOs;
using ERP.Domain.CRM.Repositories;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetAllContractsQuery.
/// </summary>
public class GetAllContractsQueryHandler : IRequestHandler<GetAllContractsQuery, IEnumerable<ContractListItemDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetAllContractsQueryHandler(
        IContractRepository contractRepository,
        IClientRepository clientRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _contractRepository = contractRepository;
        _clientRepository = clientRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<ContractListItemDto>> Handle(GetAllContractsQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var contracts = await _contractRepository.GetAllWithProjectAsync(cancellationToken);

        // Filter by status if needed
        if (query.ActiveOnly)
        {
            contracts = contracts.Where(c => c.IsActive);
        }

        // Filter by contract type if specified
        if (!string.IsNullOrEmpty(query.ContractType) && Enum.TryParse<ContractType>(query.ContractType, out var contractType))
        {
            contracts = contracts.Where(c => c.ContractType == contractType);
        }

        // Filter by client if specified
        if (query.ClientId.HasValue)
        {
            contracts = contracts.Where(c => c.Project != null && c.Project.ClientId == query.ClientId.Value);
        }

        // Get unique client IDs from contracts
        var clientIds = contracts
            .Where(c => c.Project != null)
            .Select(c => c.Project!.ClientId)
            .Distinct()
            .ToList();

        // Fetch client data
        var clients = await _clientRepository.GetAllAsync(cancellationToken);
        var clientDict = clients
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c.Name);

        return contracts.Select(c => new ContractListItemDto
        {
            Id = c.Id,
            ContractNumber = c.ContractNumber,
            Title = c.Title,
            ContractType = c.ContractType.ToString(),
            ContractValueAmount = c.ContractValue?.Amount,
            ContractValueCurrency = c.ContractValue?.Currency,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            IsActive = c.IsActive,
            ProjectId = c.ProjectId,
            ProjectNumber = c.Project?.ProjectNumber.Value ?? string.Empty,
            ProjectName = c.Project?.Name ?? string.Empty,
            ClientId = c.Project?.ClientId ?? 0,
            ClientName = c.Project != null && clientDict.TryGetValue(c.Project.ClientId, out var name) ? name : string.Empty
        });
    }
}
