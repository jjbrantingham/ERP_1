using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.Identity.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .WithMessage($"Username cannot exceed {BusinessConstants.Lengths.ShortName} characters")
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Username can only contain letters, numbers, underscores, and hyphens");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(BusinessConstants.Lengths.Email)
            .WithMessage($"Email cannot exceed {BusinessConstants.Lengths.Email} characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(BusinessConstants.Security.MinPasswordLength)
            .WithMessage($"Password must be at least {BusinessConstants.Security.MinPasswordLength} characters")
            .MaximumLength(BusinessConstants.Security.MaxPasswordLength)
            .WithMessage($"Password cannot exceed {BusinessConstants.Security.MaxPasswordLength} characters")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]")
            .WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required")
            .Equal(x => x.Password)
            .WithMessage("Passwords must match");

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

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(BusinessConstants.Lengths.Phone)
            .WithMessage($"Phone number cannot exceed {BusinessConstants.Lengths.Phone} characters")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be a valid E.164 format");
    }
}
