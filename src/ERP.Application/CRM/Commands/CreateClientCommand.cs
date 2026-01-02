using ERP.Application.Common.Interfaces;
using ERP.Domain.CRM.Enums;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Command to create a new client.
/// </summary>
public class CreateClientCommand : ICommand<long>
{
    public string Name { get; init; } = string.Empty;
    public ClientType ClientType { get; init; }
    public string? LegalName { get; init; }
    public string? TaxId { get; init; }
    public string? Website { get; init; }
    public string? Industry { get; init; }
    public string? PrimaryEmail { get; init; }
    public string? PrimaryPhone { get; init; }

    // Billing address
    public string? BillingStreet { get; init; }
    public string? BillingCity { get; init; }
    public string? BillingCountry { get; init; }
    public string? BillingStreet2 { get; init; }
    public string? BillingStateProvince { get; init; }
    public string? BillingPostalCode { get; init; }

    // Shipping address
    public string? ShippingStreet { get; init; }
    public string? ShippingCity { get; init; }
    public string? ShippingCountry { get; init; }
    public string? ShippingStreet2 { get; init; }
    public string? ShippingStateProvince { get; init; }
    public string? ShippingPostalCode { get; init; }

    public long? AccountManagerId { get; init; }
    public string? PaymentTerms { get; init; }
    public string? CreditLimit { get; init; }
    public string? Notes { get; init; }
}
