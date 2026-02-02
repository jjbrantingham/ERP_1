using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to update an existing contract.
/// </summary>
public class UpdateContractCommand : IRequest<Unit>
{
    public long Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public decimal? ContractValueAmount { get; init; }
    public string? ContractValueCurrency { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime? SignedDate { get; init; }
    public string? Terms { get; init; }
}
