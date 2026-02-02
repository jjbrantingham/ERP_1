using ERP.Domain.PM.Enums;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to create a new contract.
/// </summary>
public class CreateContractCommand : IRequest<long>
{
    public long ProjectId { get; init; }
    public string ContractNumber { get; init; } = string.Empty;
    public ContractType ContractType { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public decimal? ContractValueAmount { get; init; }
    public string? ContractValueCurrency { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime? SignedDate { get; init; }
    public string? Terms { get; init; }
}
