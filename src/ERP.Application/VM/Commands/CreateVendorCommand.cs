using ERP.Domain.VM.Enums;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Command to create a new vendor.
/// </summary>
public class CreateVendorCommand
{
    public string Name { get; set; } = string.Empty;
    public VendorType VendorType { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
}
