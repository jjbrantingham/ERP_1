using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Validator for CreateResourceTypeCommand.
/// </summary>
public class CreateResourceTypeCommandValidator : AbstractValidator<CreateResourceTypeCommand>
{
    public CreateResourceTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Resource type name is required")
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .WithMessage($"Resource type name must not exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.Description)
            .MaximumLength(BusinessConstants.Lengths.Description)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage($"Description must not exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage("Code must not exceed 50 characters")
            .Matches(@"^[A-Z0-9_-]+$")
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage("Code must contain only uppercase letters, numbers, hyphens, and underscores");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be zero or greater");
    }
}
