using FluentValidation;

namespace ERP.Application.WF.Commands;

/// <summary>
/// Validator for ActivateWorkflowDefinitionCommand.
/// </summary>
public class ActivateWorkflowDefinitionCommandValidator : AbstractValidator<ActivateWorkflowDefinitionCommand>
{
    public ActivateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId)
            .GreaterThan(0)
            .WithMessage("Workflow definition ID must be greater than zero");
    }
}
