namespace ERP.Application.CRM.DTOs;

/// <summary>
/// Data transfer object for Contact.
/// </summary>
public class ContactDto
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public string? ClientName { get; set; }
    public string ContactType { get; set; } = string.Empty;

    // Personal information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }

    // Contact information
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }

    // Flags
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }

    // Additional
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
