namespace ERP.Application.HR.DTOs;

/// <summary>
/// Data transfer object for Rate.
/// </summary>
public class RateDto
{
    public long Id { get; set; }
    public long? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public long? ResourceTypeId { get; set; }
    public string? ResourceTypeName { get; set; }
    public string RateType { get; set; } = string.Empty;
    public decimal CostRateAmount { get; set; }
    public string CostRateCurrency { get; set; } = string.Empty;
    public decimal BillingRateAmount { get; set; }
    public string BillingRateCurrency { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
