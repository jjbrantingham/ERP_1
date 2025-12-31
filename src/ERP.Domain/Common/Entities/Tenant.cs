namespace ERP.Domain.Common.Entities;

/// <summary>
/// Represents a tenant in the multi-tenant system.
/// Each tenant has isolated data and configuration.
/// </summary>
public class Tenant : AggregateRoot
{
    /// <summary>
    /// Gets the unique tenant identifier (GUID).
    /// </summary>
    public Guid TenantGuid { get; private set; }

    /// <summary>
    /// Gets the company name.
    /// </summary>
    public string CompanyName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the tenant subdomain or identifier for URL routing.
    /// </summary>
    public string Subdomain { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the tenant is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the date when the tenant subscription started.
    /// </summary>
    public DateTime SubscriptionStartDate { get; private set; }

    /// <summary>
    /// Gets the date when the tenant subscription ends (null for unlimited).
    /// </summary>
    public DateTime? SubscriptionEndDate { get; private set; }

    /// <summary>
    /// Gets the subscription plan name.
    /// </summary>
    public string SubscriptionPlan { get; private set; } = "Standard";

    /// <summary>
    /// Gets the maximum number of users allowed.
    /// </summary>
    public int MaxUsers { get; private set; }

    /// <summary>
    /// Gets the tenant settings as JSON.
    /// </summary>
    public string Settings { get; private set; } = "{}";

    /// <summary>
    /// Gets the default currency for this tenant.
    /// </summary>
    public string DefaultCurrency { get; private set; } = "USD";

    /// <summary>
    /// Gets the tenant's time zone.
    /// </summary>
    public string TimeZone { get; private set; } = "UTC";

    private Tenant() { } // EF Core

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    public static Tenant Create(
        string companyName,
        string subdomain,
        string defaultCurrency,
        int maxUsers,
        string subscriptionPlan,
        string timeZone = "UTC")
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required.", nameof(companyName));

        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain is required.", nameof(subdomain));

        if (maxUsers <= 0)
            throw new ArgumentException("Max users must be greater than zero.", nameof(maxUsers));

        var tenant = new Tenant
        {
            TenantGuid = Guid.NewGuid(),
            CompanyName = companyName,
            Subdomain = subdomain.ToLowerInvariant(),
            IsActive = true,
            SubscriptionStartDate = DateTime.UtcNow,
            SubscriptionPlan = subscriptionPlan,
            MaxUsers = maxUsers,
            DefaultCurrency = defaultCurrency,
            TimeZone = timeZone,
            CreatedDate = DateTime.UtcNow
        };

        // Set TenantId to itself for the tenant entity
        tenant.TenantId = tenant.TenantGuid;

        return tenant;
    }

    /// <summary>
    /// Activates the tenant.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Tenant is already active.");

        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the tenant.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Tenant is already inactive.");

        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the subscription.
    /// </summary>
    public void UpdateSubscription(string plan, int maxUsers, DateTime? endDate)
    {
        if (maxUsers <= 0)
            throw new ArgumentException("Max users must be greater than zero.", nameof(maxUsers));

        SubscriptionPlan = plan;
        MaxUsers = maxUsers;
        SubscriptionEndDate = endDate;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the tenant subscription is expired.
    /// </summary>
    public bool IsSubscriptionExpired()
    {
        return SubscriptionEndDate.HasValue && SubscriptionEndDate.Value < DateTime.UtcNow;
    }
}
