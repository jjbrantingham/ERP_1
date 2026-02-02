using ERP.Application.PM.DTOs;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get all contracts with project and client information.
/// </summary>
public class GetAllContractsQuery : IRequest<IEnumerable<ContractListItemDto>>
{
    public bool ActiveOnly { get; set; } = false;
    public string? ContractType { get; set; }
    public long? ClientId { get; set; }
}
