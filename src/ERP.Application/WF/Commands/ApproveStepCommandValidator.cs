using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.WF.Commands;

/// <summary>
/// Validator for ApproveStepCommand.
/// </summary>
public class ApproveStepCommandValidator : AbstractValidator<ApproveStepCommand>
{
    public ApproveStepCommandValidator()
    {
        RuleFor(x => x.WorkflowInstanceId)
            .GreaterThan(0)
            .WithMessage("Workflow instance ID must be greater than zero");

        RuleFor(x => x.StepSequenceNumber)
            .GreaterThan(0)
            .WithMessage("Step sequence number must be greater than zero");

        RuleFor(x => x.Comments)
            .MaximumLength(BusinessConstants.Lengths.Notes)
            .When(x => !string.IsNullOrEmpty(x.Comments))
            .WithMessage($"Comments must not exceed {BusinessConstants.Lengths.Notes} characters");
    }
}
