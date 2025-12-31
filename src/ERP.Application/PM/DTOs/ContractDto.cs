namespace ERP.Application.PM.DTOs;

/// <summary>
/// Data transfer object for Contract.
/// </summary>
public class ContractDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long ProjectId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? ContractValueAmount { get; set; }
    public string? ContractValueCurrency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? SignedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Terms { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
