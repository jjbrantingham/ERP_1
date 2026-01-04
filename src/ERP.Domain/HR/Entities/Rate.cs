using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.HR.Enums;

namespace ERP.Domain.HR.Entities;

/// <summary>
/// Represents a cost or billing rate for an employee or resource type.
/// Supports time-based rates (effective dates) and different rate types.
/// </summary>
public class Rate : AggregateRoot
{
    public long? EmployeeId { get; private set; }
    public long? ResourceTypeId { get; private set; }
    public RateType RateType { get; private set; }

    // Cost rate (what it costs the company)
    public Money CostRate { get; private set; }

    // Billing rate (what clients are charged)
    public Money BillingRate { get; private set; }

    public DateTime EffectiveDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public Employee? Employee { get; private set; }
    public ResourceType? ResourceType { get; private set; }

    private Rate()
    {
        CostRate = new Money(0, "USD");
        BillingRate = new Money(0, "USD");
    }

    /// <summary>
    /// Creates a rate for an employee.
    /// </summary>
    public static Rate CreateForEmployee(
        Guid tenantId,
        long employeeId,
        RateType rateType,
        Money costRate,
        Money billingRate,
        DateTime effectiveDate,
        DateTime? endDate = null,
        string? notes = null)
    {
        ValidateRates(costRate, billingRate, effectiveDate, endDate);

        var rate = new Rate
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            ResourceTypeId = null,
            RateType = rateType,
            CostRate = costRate,
            BillingRate = billingRate,
            EffectiveDate = effectiveDate,
            EndDate = endDate,
            IsActive = true,
            Notes = notes?.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        return rate;
    }

    /// <summary>
    /// Creates a rate for a resource type (default rate).
    /// </summary>
    public static Rate CreateForResourceType(
        Guid tenantId,
        long resourceTypeId,
        RateType rateType,
        Money costRate,
        Money billingRate,
        DateTime effectiveDate,
        DateTime? endDate = null,
        string? notes = null)
    {
        ValidateRates(costRate, billingRate, effectiveDate, endDate);

        var rate = new Rate
        {
            TenantId = tenantId,
            EmployeeId = null,
            ResourceTypeId = resourceTypeId,
            RateType = rateType,
            CostRate = costRate,
            BillingRate = billingRate,
            EffectiveDate = effectiveDate,
            EndDate = endDate,
            IsActive = true,
            Notes = notes?.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        return rate;
    }

    /// <summary>
    /// Updates the rate amounts.
    /// </summary>
    public void UpdateRates(Money costRate, Money billingRate, string? notes = null)
    {
        if (costRate.Amount < 0)
            throw new ArgumentException("Cost rate cannot be negative", nameof(costRate));

        if (billingRate.Amount < 0)
            throw new ArgumentException("Billing rate cannot be negative", nameof(billingRate));

        if (costRate.Currency != billingRate.Currency)
            throw new ArgumentException("Cost rate and billing rate must have the same currency");

        CostRate = costRate;
        BillingRate = billingRate;
        Notes = notes?.Trim();
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the end date for this rate (expires the rate).
    /// </summary>
    public void SetEndDate(DateTime endDate)
    {
        if (endDate < EffectiveDate)
            throw new ArgumentException("End date cannot be before effective date", nameof(endDate));

        EndDate = endDate;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the rate.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the rate.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the rate is effective on a given date.
    /// </summary>
    public bool IsEffectiveOn(DateTime date)
    {
        return IsActive &&
               date >= EffectiveDate &&
               (!EndDate.HasValue || date <= EndDate.Value);
    }

    private static void ValidateRates(Money costRate, Money billingRate, DateTime effectiveDate, DateTime? endDate)
    {
        if (costRate.Amount < 0)
            throw new ArgumentException("Cost rate cannot be negative", nameof(costRate));

        if (billingRate.Amount < 0)
            throw new ArgumentException("Billing rate cannot be negative", nameof(billingRate));

        if (costRate.Currency != billingRate.Currency)
            throw new ArgumentException("Cost rate and billing rate must have the same currency");

        if (endDate.HasValue && endDate.Value < effectiveDate)
            throw new ArgumentException("End date cannot be before effective date", nameof(endDate));
    }
}
