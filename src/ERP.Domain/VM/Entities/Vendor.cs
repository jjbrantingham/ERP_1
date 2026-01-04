using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.VM.Enums;
using ERP.Domain.VM.ValueObjects;

namespace ERP.Domain.VM.Entities;

/// <summary>
/// Vendor aggregate root representing a company or individual that provides goods or services.
/// </summary>
public class Vendor : AggregateRoot
{
    public long Id { get; private set; }
    public VendorNumber VendorNumber { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public VendorType VendorType { get; private set; }
    public VendorStatus Status { get; private set; }
    public string? TaxId { get; private set; }
    public string? Website { get; private set; }
    public Email? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Fax { get; private set; }

    // Address (flattened value object)
    public string? AddressStreet { get; private set; }
    public string? AddressStreet2 { get; private set; }
    public string? AddressCity { get; private set; }
    public string? AddressStateProvince { get; private set; }
    public string? AddressPostalCode { get; private set; }
    public string? AddressCountry { get; private set; }

    // Payment terms
    public string? PaymentTerms { get; private set; }
    public int? PaymentDueDays { get; private set; }
    public string? PreferredPaymentMethod { get; private set; }

    // Relationships
    public string? AccountNumber { get; private set; }
    public decimal? CreditLimit { get; private set; }
    public string? Notes { get; private set; }

    // Audit fields
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public byte[]? RowVersion { get; private set; }

    private Vendor() { } // EF Core

    private Vendor(
        Guid tenantId,
        VendorNumber vendorNumber,
        string name,
        VendorType vendorType)
    {
        TenantId = tenantId;
        VendorNumber = vendorNumber;
        Name = name;
        VendorType = vendorType;
        Status = VendorStatus.Prospective;
        IsActive = true;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new vendor.
    /// </summary>
    public static Vendor Create(
        Guid tenantId,
        VendorNumber vendorNumber,
        string name,
        VendorType vendorType,
        string? taxId = null,
        string? website = null,
        Email? email = null,
        string? phone = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vendor name is required", nameof(name));

        var vendor = new Vendor(tenantId, vendorNumber, name, vendorType)
        {
            TaxId = taxId,
            Website = website,
            Email = email,
            Phone = phone,
            Notes = notes
        };

        return vendor;
    }

    /// <summary>
    /// Updates vendor basic information.
    /// </summary>
    public void UpdateBasicInfo(
        string name,
        VendorType vendorType,
        string? taxId = null,
        string? website = null,
        string? phone = null,
        string? fax = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vendor name is required", nameof(name));

        Name = name;
        VendorType = vendorType;
        TaxId = taxId;
        Website = website;
        Phone = phone;
        Fax = fax;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the vendor's address.
    /// </summary>
    public void SetAddress(
        string? street,
        string? city,
        string? country,
        string? street2 = null,
        string? stateProvince = null,
        string? postalCode = null)
    {
        AddressStreet = street;
        AddressStreet2 = street2;
        AddressCity = city;
        AddressStateProvince = stateProvince;
        AddressPostalCode = postalCode;
        AddressCountry = country;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets payment terms for the vendor.
    /// </summary>
    public void SetPaymentTerms(
        string? paymentTerms,
        int? paymentDueDays = null,
        string? preferredPaymentMethod = null,
        decimal? creditLimit = null)
    {
        PaymentTerms = paymentTerms;
        PaymentDueDays = paymentDueDays;
        PreferredPaymentMethod = preferredPaymentMethod;
        CreditLimit = creditLimit;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes vendor status.
    /// </summary>
    public void ChangeStatus(VendorStatus newStatus)
    {
        if (Status == newStatus)
            return;

        var oldStatus = Status;
        Status = newStatus;
        ModifiedDate = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new VendorStatusChangedEvent(Id, TenantId, oldStatus, newStatus));
    }

    /// <summary>
    /// Activates the vendor.
    /// </summary>
    public void Activate()
    {
        if (Status == VendorStatus.Blacklisted)
            throw new InvalidOperationException("Cannot activate a blacklisted vendor");

        ChangeStatus(VendorStatus.Active);
    }

    /// <summary>
    /// Deactivates the vendor.
    /// </summary>
    public void Deactivate()
    {
        ChangeStatus(VendorStatus.Inactive);
    }

    /// <summary>
    /// Blacklists the vendor.
    /// </summary>
    public void Blacklist(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Blacklist reason is required", nameof(reason));

        Status = VendorStatus.Blacklisted;
        Notes = $"BLACKLISTED: {reason}\n\n{Notes}";
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }
}

/// <summary>
/// Domain event raised when vendor status changes.
/// </summary>
public class VendorStatusChangedEvent : DomainEvent
{
    public long VendorId { get; }
    public Guid TenantId { get; }
    public VendorStatus OldStatus { get; }
    public VendorStatus NewStatus { get; }
    public DateTime ChangedAt { get; }

    public VendorStatusChangedEvent(
        long vendorId,
        Guid tenantId,
        VendorStatus oldStatus,
        VendorStatus newStatus)
    {
        VendorId = vendorId;
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = DateTime.UtcNow;
    }
}
