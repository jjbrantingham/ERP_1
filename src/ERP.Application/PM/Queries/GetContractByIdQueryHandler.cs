using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.PM.DTOs;
using ERP.Domain.CRM.Repositories;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetContractByIdQuery.
/// </summary>
public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractDetailDto?>
{
    private readonly IContractRepository _contractRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetContractByIdQueryHandler(
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

    public async Task<ContractDetailDto?> Handle(GetContractByIdQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var contract = await _contractRepository.GetByIdWithProjectAsync(query.ContractId, cancellationToken);
        if (contract == null)
            return null;

        // Get client info if project exists
        string clientName = string.Empty;
        if (contract.Project != null)
        {
            var client = await _clientRepository.GetByIdAsync(contract.Project.ClientId, cancellationToken);
            clientName = client?.Name ?? string.Empty;
        }

        return new ContractDetailDto
        {
            Id = contract.Id,
            TenantId = contract.TenantId,
            ContractNumber = contract.ContractNumber,
            ContractType = contract.ContractType.ToString(),
            Title = contract.Title,
            Description = contract.Description,
            ContractValueAmount = contract.ContractValue?.Amount,
            ContractValueCurrency = contract.ContractValue?.Currency,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            SignedDate = contract.SignedDate,
            IsActive = contract.IsActive,
            Terms = contract.Terms,
            CreatedDate = contract.CreatedDate,
            ModifiedDate = contract.ModifiedDate,
            ProjectId = contract.ProjectId,
            ProjectNumber = contract.Project?.ProjectNumber.Value ?? string.Empty,
            ProjectName = contract.Project?.Name ?? string.Empty,
            ProjectStatus = contract.Project?.Status.ToString() ?? string.Empty,
            ClientId = contract.Project?.ClientId ?? 0,
            ClientName = clientName
        };
    }
}
