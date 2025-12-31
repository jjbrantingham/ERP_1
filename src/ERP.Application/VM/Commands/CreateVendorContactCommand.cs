using ERP.Domain.VM.Enums;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Command to create a new vendor contact.
/// </summary>
public class CreateVendorContactCommand
{
    public long VendorId { get; set; }
    public ContactType ContactType { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public bool IsPrimary { get; set; }
    public string? Notes { get; set; }
}
