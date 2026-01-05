using ERP.Application.Common.Interfaces;
namespace ERP.Application.PM.DTOs;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get contracts for a project.
/// </summary>
public class GetProjectContractsQuery : IQuery<IEnumerable<ContractDto>
{
    public long ProjectId { get; set; }
}
