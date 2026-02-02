using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.VM.Enums;

namespace ERP.Domain.VM.Entities;

/// <summary>
/// Contact person for a vendor.
/// </summary>
public class VendorContact : AggregateRoot
{
    public long VendorId { get; private set; }
    public ContactType ContactType { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Title { get; private set; }
    public Email Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? Mobile { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? Notes { get; private set; }

    // Navigation property
    public Vendor Vendor { get; private set; } = null!;

    // Additional fields
    public bool IsActive { get; private set; }

    private VendorContact() { } // EF Core

    private VendorContact(
        Guid tenantId,
        long vendorId,
        ContactType contactType,
        string firstName,
        string lastName,
        Email email)
    {
        TenantId = tenantId;
        VendorId = vendorId;
        ContactType = contactType;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        IsPrimary = false;
        IsActive = true;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new vendor contact.
    /// </summary>
    public static VendorContact Create(
        Guid tenantId,
        long vendorId,
        ContactType contactType,
        string firstName,
        string lastName,
        Email email,
        string? title = null,
        string? phone = null,
        string? mobile = null,
        bool isPrimary = false,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        var contact = new VendorContact(tenantId, vendorId, contactType, firstName, lastName, email)
        {
            Title = title,
            Phone = phone,
            Mobile = mobile,
            IsPrimary = isPrimary,
            Notes = notes
        };

        return contact;
    }

    /// <summary>
    /// Updates contact information.
    /// </summary>
    public void UpdateInfo(
        string firstName,
        string lastName,
        Email email,
        ContactType contactType,
        string? title = null,
        string? phone = null,
        string? mobile = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        ContactType = contactType;
        Title = title;
        Phone = phone;
        Mobile = mobile;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets as primary contact.
    /// </summary>
    public void SetAsPrimary()
    {
        IsPrimary = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes primary designation.
    /// </summary>
    public void RemovePrimary()
    {
        IsPrimary = false;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the full name of the contact.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
