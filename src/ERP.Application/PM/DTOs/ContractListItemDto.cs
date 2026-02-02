namespace ERP.Application.PM.DTOs;

/// <summary>
/// Data transfer object for Contract list items with project and client information.
/// </summary>
public class ContractListItemDto
{
    public long Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public decimal? ContractValueAmount { get; set; }
    public string? ContractValueCurrency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    // Project info
    public long ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;

    // Client info (from Project)
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
}
