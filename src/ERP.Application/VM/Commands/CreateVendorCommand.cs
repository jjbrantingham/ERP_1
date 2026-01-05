using ERP.Application.Common.Interfaces;
using ERP.Domain.VM.Enums;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Command to create a new vendor.
/// </summary>
public class CreateVendorCommand : ICommand<long>
{
    public string Name { get; init; } = string.Empty;
    public VendorType VendorType { get; init; }
    public string? TaxId { get; init; }
    public string? Website { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
}
