using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.Identity.Commands;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than zero");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(BusinessConstants.Security.MinPasswordLength)
            .WithMessage($"New password must be at least {BusinessConstants.Security.MinPasswordLength} characters")
            .MaximumLength(BusinessConstants.Security.MaxPasswordLength)
            .WithMessage($"New password cannot exceed {BusinessConstants.Security.MaxPasswordLength} characters")
            .Matches(@"[A-Z]")
            .WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("New password must contain at least one number")
            .Matches(@"[\W_]")
            .WithMessage("New password must contain at least one special character")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from current password");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords must match");
    }
}
