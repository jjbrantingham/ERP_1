using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.BILL.Commands;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage("Client ID must be greater than zero");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .When(x => x.ProjectId.HasValue)
            .WithMessage("Project ID must be greater than zero when provided");

        RuleFor(x => x.BillingMode)
            .NotEmpty()
            .WithMessage("Billing mode is required")
            .Must(mode => new[] { "TimeAndMaterials", "FixedPrice", "Milestone", "PercentComplete" }.Contains(mode))
            .WithMessage("Billing mode must be one of: TimeAndMaterials, FixedPrice, Milestone, PercentComplete");

        RuleFor(x => x.InvoiceDate)
            .NotEmpty()
            .WithMessage("Invoice date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(BusinessConstants.DateTime.MaxFutureDays))
            .WithMessage($"Invoice date cannot be more than {BusinessConstants.DateTime.MaxFutureDays} day in the future");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage("Due date is required")
            .GreaterThanOrEqualTo(x => x.InvoiceDate)
            .WithMessage("Due date must be on or after invoice date");

        RuleFor(x => x.PoNumber)
            .MaximumLength(BusinessConstants.Lengths.ReferenceNumber)
            .WithMessage($"PO number cannot exceed {BusinessConstants.Lengths.ReferenceNumber} characters");

        RuleFor(x => x.Description)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .WithMessage($"Description cannot exceed {BusinessConstants.Lengths.LongDescription} characters");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Tax rate must be between 0 and 1 (0% to 100%)");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .WithMessage($"Currency must be a {BusinessConstants.Currency.CurrencyCodeLength}-letter ISO code (e.g., USD, EUR)")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency))
            .WithMessage("Currency must be a supported currency code");

        RuleFor(x => x.LineItems)
            .NotEmpty()
            .WithMessage("At least one line item is required")
            .Must(items => items != null && items.Count <= BusinessConstants.Limits.MaxInvoiceLineItems)
            .WithMessage($"Cannot exceed {BusinessConstants.Limits.MaxInvoiceLineItems} line items");

        RuleForEach(x => x.LineItems)
            .SetValidator(new InvoiceLineItemCommandValidator());
    }
}

public class InvoiceLineItemCommandValidator : AbstractValidator<InvoiceLineItemCommand>
{
    public InvoiceLineItemCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Line item description is required")
            .MaximumLength(BusinessConstants.Lengths.Description)
            .WithMessage($"Line item description cannot exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit price must be greater than or equal to zero");

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(BusinessConstants.Limits.MinDiscountPercent, BusinessConstants.Limits.MaxDiscountPercent)
            .WithMessage($"Discount percent must be between {BusinessConstants.Limits.MinDiscountPercent} and {BusinessConstants.Limits.MaxDiscountPercent}");
    }
}
