namespace ERP.Application.VM.DTOs;

/// <summary>
/// Data transfer object for Vendor Contact.
/// </summary>
public class VendorContactDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long VendorId { get; set; }
    public string ContactType { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public bool IsPrimary { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
