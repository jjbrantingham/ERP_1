using ERP.Domain.PM.Enums;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to create a new contract.
/// </summary>
public class CreateContractCommand
{
    public long ProjectId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public ContractType ContractType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? ContractValueAmount { get; set; }
    public string? ContractValueCurrency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? SignedDate { get; set; }
    public string? Terms { get; set; }
}
