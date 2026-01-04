using FluentValidation;

namespace ERP.Application.WF.Commands;

/// <summary>
/// Validator for StartWorkflowCommand.
/// </summary>
public class StartWorkflowCommandValidator : AbstractValidator<StartWorkflowCommand>
{
    public StartWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId)
            .GreaterThan(0)
            .WithMessage("Workflow definition ID must be greater than zero");

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("Entity type is required")
            .MaximumLength(100)
            .WithMessage("Entity type must not exceed 100 characters");

        RuleFor(x => x.EntityId)
            .GreaterThan(0)
            .WithMessage("Entity ID must be greater than zero");
    }
}
