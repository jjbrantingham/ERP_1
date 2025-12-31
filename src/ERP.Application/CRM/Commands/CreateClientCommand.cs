using ERP.Application.Common.Interfaces;
using ERP.Domain.CRM.Enums;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Command to create a new client.
/// </summary>
public class CreateClientCommand : ICommand<long>
{
    public string Name { get; set; } = string.Empty;
    public ClientType ClientType { get; set; }
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }

    // Billing address
    public string? BillingStreet { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingStreet2 { get; set; }
    public string? BillingStateProvince { get; set; }
    public string? BillingPostalCode { get; set; }

    // Shipping address
    public string? ShippingStreet { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingStreet2 { get; set; }
    public string? ShippingStateProvince { get; set; }
    public string? ShippingPostalCode { get; set; }

    public long? AccountManagerId { get; set; }
    public string? PaymentTerms { get; set; }
    public string? CreditLimit { get; set; }
    public string? Notes { get; set; }
}
