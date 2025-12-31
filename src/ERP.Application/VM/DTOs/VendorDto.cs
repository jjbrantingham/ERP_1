namespace ERP.Application.VM.DTOs;

/// <summary>
/// Data transfer object for Vendor.
/// </summary>
public class VendorDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public string VendorNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressStreet2 { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressStateProvince { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; }
    public string? PaymentTerms { get; set; }
    public int? PaymentDueDays { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public string? AccountNumber { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
