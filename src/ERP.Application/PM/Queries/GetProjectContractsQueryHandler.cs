using ERP.Application.Common.Interfaces;
using ERP.Application.PM.DTOs;
using ERP.Domain.PM.Repositories;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetProjectContractsQuery.
/// </summary>
public class GetProjectContractsQueryHandler : IQueryHandler<GetProjectContractsQuery, IEnumerable<ContractDto>>
{
    private readonly IContractRepository _contractRepository;

    public GetProjectContractsQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<IEnumerable<ContractDto>> Handle(GetProjectContractsQuery query, CancellationToken cancellationToken = default)
    {
        var contracts = await _contractRepository.GetByProjectIdAsync(query.ProjectId, cancellationToken);

        return contracts.Select(c => new ContractDto
        {
            Id = c.Id,
            TenantId = c.TenantId,
            ProjectId = c.ProjectId,
            ContractNumber = c.ContractNumber,
            ContractType = c.ContractType.ToString(),
            Title = c.Title,
            Description = c.Description,
            ContractValueAmount = c.ContractValue?.Amount,
            ContractValueCurrency = c.ContractValue?.Currency,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            SignedDate = c.SignedDate,
            IsActive = c.IsActive,
            Terms = c.Terms,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate
        });
    }
}
