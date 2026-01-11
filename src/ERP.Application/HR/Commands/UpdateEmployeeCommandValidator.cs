using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Validator for UpdateEmployeeCommand.
/// </summary>
public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than zero");

        RuleFor(x => x.ResourceTypeId)
            .GreaterThan(0)
            .WithMessage("Resource Type ID must be greater than zero");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .WithMessage($"First name must not exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .WithMessage($"Last name must not exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.MiddleName)
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .When(x => !string.IsNullOrEmpty(x.MiddleName))
            .WithMessage($"Middle name must not exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(BusinessConstants.Lengths.Email)
            .WithMessage($"Email must not exceed {BusinessConstants.Lengths.Email} characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number must be in E.164 format (e.g., +12125551234)");

        RuleFor(x => x.MobileNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.MobileNumber))
            .WithMessage("Mobile number must be in E.164 format (e.g., +12125551234)");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-18))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Employee must be at least 18 years old");

        RuleFor(x => x.EmploymentType)
            .IsInEnum()
            .WithMessage("Invalid employment type");

        RuleFor(x => x.JobTitle)
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .When(x => !string.IsNullOrEmpty(x.JobTitle))
            .WithMessage($"Job title must not exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.Department)
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .When(x => !string.IsNullOrEmpty(x.Department))
            .WithMessage($"Department must not exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0)
            .When(x => x.ManagerId.HasValue)
            .WithMessage("Manager ID must be greater than zero if specified")
            .NotEqual(x => x.EmployeeId)
            .When(x => x.ManagerId.HasValue)
            .WithMessage("Employee cannot be their own manager");

        RuleFor(x => x.BaseSalaryAmount)
            .GreaterThan(0)
            .When(x => x.BaseSalaryAmount.HasValue)
            .WithMessage("Base salary amount must be greater than zero if specified")
            .PrecisionScale(BusinessConstants.Currency.MoneyPrecision, BusinessConstants.Currency.MoneyScale, ignoreTrailingZeros: true)
            .When(x => x.BaseSalaryAmount.HasValue)
            .WithMessage($"Base salary amount must have at most {BusinessConstants.Currency.MoneyScale} decimal places");

        RuleFor(x => x.BaseSalaryCurrency)
            .NotEmpty()
            .When(x => x.BaseSalaryAmount.HasValue)
            .WithMessage("Base salary currency is required when salary amount is specified")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .When(x => !string.IsNullOrEmpty(x.BaseSalaryCurrency))
            .WithMessage($"Currency code must be exactly {BusinessConstants.Currency.CurrencyCodeLength} characters")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency))
            .When(x => !string.IsNullOrEmpty(x.BaseSalaryCurrency))
            .WithMessage(x => $"Currency '{x.BaseSalaryCurrency}' is not supported. Supported currencies: {string.Join(", ", BusinessConstants.Currency.SupportedCurrencies)}");

        RuleFor(x => x.StandardHoursPerWeek)
            .InclusiveBetween(1, 168)
            .WithMessage("Standard hours per week must be between 1 and 168 (hours in a week)");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage($"Notes must not exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
