namespace ERP.Application.CRM.DTOs;

/// <summary>
/// Data transfer object for Client.
/// </summary>
public class ClientDto
{
    public long Id { get; set; }
    public string ClientNumber { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // Organization information
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }

    // Contact information
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }

    // Billing address
    public string? BillingStreet { get; set; }
    public string? BillingStreet2 { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingStateProvince { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }

    // Shipping address
    public string? ShippingStreet { get; set; }
    public string? ShippingStreet2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingStateProvince { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }

    // Business relationship
    public DateTime? FirstContactDate { get; set; }
    public DateTime? LastContactDate { get; set; }
    public long? AccountManagerId { get; set; }
    public string? AccountManagerName { get; set; }

    // Financial
    public string? PaymentTerms { get; set; }
    public string? CreditLimit { get; set; }

    // Additional
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    // Audit
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
