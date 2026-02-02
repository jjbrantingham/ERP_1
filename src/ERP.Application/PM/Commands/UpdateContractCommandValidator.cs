using FluentValidation;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Validator for UpdateContractCommand.
/// </summary>
public class UpdateContractCommandValidator : AbstractValidator<UpdateContractCommand>
{
    public UpdateContractCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Contract ID must be greater than 0");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 4000 characters");

        RuleFor(x => x.ContractValueAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ContractValueAmount.HasValue)
            .WithMessage("Contract value must be non-negative");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Terms)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Terms))
            .WithMessage("Terms cannot exceed 4000 characters");
    }
}
