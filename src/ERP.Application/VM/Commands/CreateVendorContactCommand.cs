using ERP.Domain.VM.Enums;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Command to create a new vendor contact.
/// </summary>
public class CreateVendorContactCommand
{
    public long VendorId { get; init; }
    public ContactType ContactType { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Phone { get; init; }
    public string? Mobile { get; init; }
    public bool IsPrimary { get; init; }
    public string? Notes { get; init; }
}
