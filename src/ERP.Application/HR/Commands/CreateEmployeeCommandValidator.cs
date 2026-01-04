using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.HR.Commands;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.ResourceTypeId)
            .GreaterThan(0)
            .WithMessage("Resource type ID must be greater than zero");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .WithMessage($"First name cannot exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .WithMessage($"Last name cannot exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.MiddleName)
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .WithMessage($"Middle name cannot exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(BusinessConstants.Lengths.Email)
            .WithMessage($"Email cannot exceed {BusinessConstants.Lengths.Email} characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(BusinessConstants.Lengths.Phone)
            .WithMessage($"Phone number cannot exceed {BusinessConstants.Lengths.Phone} characters")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be a valid E.164 format");

        RuleFor(x => x.MobileNumber)
            .MaximumLength(BusinessConstants.Lengths.Phone)
            .WithMessage($"Mobile number cannot exceed {BusinessConstants.Lengths.Phone} characters")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber))
            .WithMessage("Mobile number must be a valid E.164 format");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-18))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Employee must be at least 18 years old");

        RuleFor(x => x.EmploymentType)
            .IsInEnum()
            .WithMessage("Employment type must be a valid value");

        RuleFor(x => x.HireDate)
            .NotEmpty()
            .WithMessage("Hire date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(BusinessConstants.DateTime.MaxFutureDays))
            .WithMessage($"Hire date cannot be more than {BusinessConstants.DateTime.MaxFutureDays} day in the future");

        RuleFor(x => x.JobTitle)
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .WithMessage($"Job title cannot exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.Department)
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .WithMessage($"Department cannot exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0)
            .When(x => x.ManagerId.HasValue)
            .WithMessage("Manager ID must be greater than zero when provided");

        RuleFor(x => x.BaseSalaryAmount)
            .GreaterThan(0)
            .When(x => x.BaseSalaryAmount.HasValue)
            .WithMessage("Base salary amount must be greater than zero");

        RuleFor(x => x.BaseSalaryCurrency)
            .NotEmpty()
            .When(x => x.BaseSalaryAmount.HasValue)
            .WithMessage("Base salary currency is required when salary amount is specified")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .When(x => !string.IsNullOrWhiteSpace(x.BaseSalaryCurrency))
            .WithMessage($"Currency must be a {BusinessConstants.Currency.CurrencyCodeLength}-letter ISO code")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency!))
            .When(x => !string.IsNullOrWhiteSpace(x.BaseSalaryCurrency))
            .WithMessage("Currency must be a supported currency code");

        RuleFor(x => x.StandardHoursPerWeek)
            .InclusiveBetween(1, 168)
            .WithMessage("Standard hours per week must be between 1 and 168");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .When(x => x.UserId.HasValue)
            .WithMessage("User ID must be greater than zero when provided");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .WithMessage($"Notes cannot exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
