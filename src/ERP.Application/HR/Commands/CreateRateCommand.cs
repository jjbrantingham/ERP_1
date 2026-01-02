using ERP.Application.Common.Interfaces;
using ERP.Domain.HR.Enums;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Command to create a new rate for an employee or resource type.
/// </summary>
public class CreateRateCommand : ICommand<long>
{
    public long? EmployeeId { get; init; }
    public long? ResourceTypeId { get; init; }
    public RateType RateType { get; init; }
    public decimal CostRateAmount { get; init; }
    public string CostRateCurrency { get; init; } = "USD";
    public decimal BillingRateAmount { get; init; }
    public string BillingRateCurrency { get; init; } = "USD";
    public DateTime EffectiveDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Notes { get; init; }
}
