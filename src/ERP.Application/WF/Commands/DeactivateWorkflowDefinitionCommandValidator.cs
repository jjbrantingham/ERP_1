using FluentValidation;

namespace ERP.Application.WF.Commands;

/// <summary>
/// Validator for DeactivateWorkflowDefinitionCommand.
/// </summary>
public class DeactivateWorkflowDefinitionCommandValidator : AbstractValidator<DeactivateWorkflowDefinitionCommand>
{
    public DeactivateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId)
            .GreaterThan(0)
            .WithMessage("Workflow definition ID must be greater than zero");
    }
}
