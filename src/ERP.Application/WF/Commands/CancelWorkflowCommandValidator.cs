using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.WF.Commands;

/// <summary>
/// Validator for CancelWorkflowCommand.
/// </summary>
public class CancelWorkflowCommandValidator : AbstractValidator<CancelWorkflowCommand>
{
    public CancelWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowInstanceId)
            .GreaterThan(0)
            .WithMessage("Workflow instance ID must be greater than zero");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required when canceling a workflow")
            .MaximumLength(BusinessConstants.Lengths.Description)
            .WithMessage($"Reason must not exceed {BusinessConstants.Lengths.Description} characters");
    }
}
