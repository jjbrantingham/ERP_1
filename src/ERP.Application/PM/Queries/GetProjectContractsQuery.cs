using ERP.Application.PM.DTOs;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get contracts for a project.
/// </summary>
public class GetProjectContractsQuery : IRequest<IEnumerable<ContractDto>>
{
    public long ProjectId { get; set; }
}
