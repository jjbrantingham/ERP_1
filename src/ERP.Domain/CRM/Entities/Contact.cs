using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.CRM.Enums;

namespace ERP.Domain.CRM.Entities;

/// <summary>
/// Represents a contact person for a client.
/// </summary>
public class Contact : AggregateRoot
{
    public long ClientId { get; private set; }
    public ContactType ContactType { get; private set; }

    // Personal information
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? MiddleName { get; private set; }
    public string? JobTitle { get; private set; }
    public string? Department { get; private set; }

    // Contact information
    public Email Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? MobileNumber { get; private set; }

    // Flags
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; }

    // Additional
    public string? Notes { get; private set; }

    // Navigation property
    public Client? Client { get; private set; }

    // Computed property
    public string FullName => $"{FirstName} {LastName}";

    private Contact()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = new Email("temp@temp.com");
    }

    /// <summary>
    /// Creates a new contact.
    /// </summary>
    public static Contact Create(
        Guid tenantId,
        long clientId,
        string firstName,
        string lastName,
        Email email,
        ContactType contactType,
        string? middleName = null,
        string? jobTitle = null,
        string? department = null,
        string? phoneNumber = null,
        string? mobileNumber = null,
        bool isPrimary = false,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        var contact = new Contact
        {
            TenantId = tenantId,
            ClientId = clientId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            MiddleName = middleName?.Trim(),
            Email = email,
            ContactType = contactType,
            JobTitle = jobTitle?.Trim(),
            Department = department?.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            MobileNumber = mobileNumber?.Trim(),
            IsPrimary = isPrimary,
            IsActive = true,
            Notes = notes?.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        return contact;
    }

    /// <summary>
    /// Updates contact information.
    /// </summary>
    public void Update(
        string firstName,
        string lastName,
        Email email,
        ContactType contactType,
        string? middleName = null,
        string? jobTitle = null,
        string? department = null,
        string? phoneNumber = null,
        string? mobileNumber = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        MiddleName = middleName?.Trim();
        Email = email;
        ContactType = contactType;
        JobTitle = jobTitle?.Trim();
        Department = department?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        MobileNumber = mobileNumber?.Trim();
        Notes = notes?.Trim();
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
    /// Removes primary status.
    /// </summary>
    public void RemovePrimary()
    {
        IsPrimary = false;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the contact.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the contact.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }
}
