using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Validator for CreateClientCommand.
/// </summary>
public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Client name is required")
            .MaximumLength(BusinessConstants.Lengths.Name)
            .WithMessage($"Client name must not exceed {BusinessConstants.Lengths.Name} characters");

        RuleFor(x => x.ClientType)
            .IsInEnum()
            .WithMessage("Invalid client type");

        RuleFor(x => x.LegalName)
            .MaximumLength(BusinessConstants.Lengths.Name)
            .When(x => !string.IsNullOrEmpty(x.LegalName))
            .WithMessage($"Legal name must not exceed {BusinessConstants.Lengths.Name} characters");

        RuleFor(x => x.TaxId)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.TaxId))
            .WithMessage("Tax ID must not exceed 50 characters");

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Website must not exceed 200 characters")
            .Matches(@"^https?://")
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Website must start with http:// or https://");

        RuleFor(x => x.Industry)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Industry))
            .WithMessage("Industry must not exceed 100 characters");

        RuleFor(x => x.PrimaryEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.PrimaryEmail))
            .WithMessage("Primary email must be a valid email address")
            .MaximumLength(BusinessConstants.Lengths.Email)
            .When(x => !string.IsNullOrEmpty(x.PrimaryEmail))
            .WithMessage($"Primary email must not exceed {BusinessConstants.Lengths.Email} characters");

        RuleFor(x => x.PrimaryPhone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PrimaryPhone))
            .WithMessage("Primary phone must be in E.164 format (e.g., +12125551234)");

        // Billing address validation
        RuleFor(x => x.BillingStreet)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.BillingStreet))
            .WithMessage("Billing street must not exceed 200 characters");

        RuleFor(x => x.BillingCity)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.BillingCity))
            .WithMessage("Billing city must not exceed 100 characters");

        RuleFor(x => x.BillingStateProvince)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.BillingStateProvince))
            .WithMessage("Billing state/province must not exceed 100 characters");

        RuleFor(x => x.BillingPostalCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.BillingPostalCode))
            .WithMessage("Billing postal code must not exceed 20 characters");

        RuleFor(x => x.BillingCountry)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.BillingCountry))
            .WithMessage("Billing country must not exceed 100 characters");

        // Shipping address validation
        RuleFor(x => x.ShippingStreet)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.ShippingStreet))
            .WithMessage("Shipping street must not exceed 200 characters");

        RuleFor(x => x.ShippingCity)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ShippingCity))
            .WithMessage("Shipping city must not exceed 100 characters");

        RuleFor(x => x.ShippingStateProvince)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ShippingStateProvince))
            .WithMessage("Shipping state/province must not exceed 100 characters");

        RuleFor(x => x.ShippingPostalCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.ShippingPostalCode))
            .WithMessage("Shipping postal code must not exceed 20 characters");

        RuleFor(x => x.ShippingCountry)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ShippingCountry))
            .WithMessage("Shipping country must not exceed 100 characters");

        RuleFor(x => x.AccountManagerId)
            .GreaterThan(0)
            .When(x => x.AccountManagerId.HasValue)
            .WithMessage("Account manager ID must be greater than zero if specified");

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms))
            .WithMessage("Payment terms must not exceed 100 characters");

        RuleFor(x => x.CreditLimit)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.CreditLimit))
            .WithMessage("Credit limit must not exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.Notes)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage($"Notes must not exceed {BusinessConstants.Lengths.Notes} characters");
    }
}
