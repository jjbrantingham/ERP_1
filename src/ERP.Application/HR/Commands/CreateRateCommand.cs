using ERP.Application.Common.Interfaces;
using ERP.Domain.HR.Enums;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Command to create a new rate for an employee or resource type.
/// </summary>
public class CreateRateCommand : ICommand<long>
{
    public long? EmployeeId { get; set; }
    public long? ResourceTypeId { get; set; }
    public RateType RateType { get; set; }
    public decimal CostRateAmount { get; set; }
    public string CostRateCurrency { get; set; } = "USD";
    public decimal BillingRateAmount { get; set; }
    public string BillingRateCurrency { get; set; } = "USD";
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
