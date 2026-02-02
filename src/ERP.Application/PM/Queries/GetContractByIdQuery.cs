using ERP.Application.PM.DTOs;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get a contract by ID with project and client information.
/// </summary>
public class GetContractByIdQuery : IRequest<ContractDetailDto?>
{
    public long ContractId { get; set; }
}
