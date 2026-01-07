using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.WF.Commands;

/// <summary>
/// Validator for RejectStepCommand.
/// </summary>
public class RejectStepCommandValidator : AbstractValidator<RejectStepCommand>
{
    public RejectStepCommandValidator()
    {
        RuleFor(x => x.WorkflowInstanceId)
            .GreaterThan(0)
            .WithMessage("Workflow instance ID must be greater than zero");

        RuleFor(x => x.StepSequenceNumber)
            .GreaterThan(0)
            .WithMessage("Step sequence number must be greater than zero");

        RuleFor(x => x.Comments)
            .NotEmpty()
            .WithMessage("Comments are required when rejecting a workflow step")
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .WithMessage($"Comments must not exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
