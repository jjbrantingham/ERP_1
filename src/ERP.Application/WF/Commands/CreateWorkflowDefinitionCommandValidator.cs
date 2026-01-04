using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.WF.Commands;

/// <summary>
/// Validator for CreateWorkflowDefinitionCommand.
/// </summary>
public class CreateWorkflowDefinitionCommandValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Workflow name is required")
            .MaximumLength(BusinessConstants.Lengths.Name)
            .WithMessage($"Workflow name must not exceed {BusinessConstants.Lengths.Name} characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Workflow description is required")
            .MaximumLength(BusinessConstants.Lengths.Description)
            .WithMessage($"Workflow description must not exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("Entity type is required")
            .MaximumLength(100)
            .WithMessage("Entity type must not exceed 100 characters");

        RuleFor(x => x.Steps)
            .NotEmpty()
            .WithMessage("At least one workflow step is required")
            .Must(steps => steps.Count <= 50)
            .WithMessage("Workflow cannot have more than 50 steps");

        RuleForEach(x => x.Steps)
            .SetValidator(new WorkflowStepCommandValidator());

        // Validate sequence numbers are unique and sequential
        RuleFor(x => x.Steps)
            .Must(HaveUniqueSequenceNumbers)
            .When(x => x.Steps != null && x.Steps.Any())
            .WithMessage("Workflow step sequence numbers must be unique");
    }

    private bool HaveUniqueSequenceNumbers(List<WorkflowStepCommand> steps)
    {
        if (steps == null || !steps.Any()) return true;
        var sequenceNumbers = steps.Select(s => s.SequenceNumber).ToList();
        return sequenceNumbers.Count == sequenceNumbers.Distinct().Count();
    }
}

/// <summary>
/// Validator for WorkflowStepCommand.
/// </summary>
public class WorkflowStepCommandValidator : AbstractValidator<WorkflowStepCommand>
{
    public WorkflowStepCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Step name is required")
            .MaximumLength(BusinessConstants.Lengths.Name)
            .WithMessage($"Step name must not exceed {BusinessConstants.Lengths.Name} characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Step description is required")
            .MaximumLength(BusinessConstants.Lengths.Description)
            .WithMessage($"Step description must not exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.StepType)
            .InclusiveBetween((byte)1, (byte)5)
            .WithMessage("Step type must be between 1 and 5");

        RuleFor(x => x.SequenceNumber)
            .GreaterThan(0)
            .WithMessage("Sequence number must be greater than zero");

        RuleFor(x => x.ApproverRole)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ApproverRole))
            .WithMessage("Approver role must not exceed 100 characters");

        RuleFor(x => x.ApproverId)
            .GreaterThan(0)
            .When(x => x.ApproverId.HasValue)
            .WithMessage("Approver ID must be greater than zero if specified");

        RuleFor(x => x.TimeoutHours)
            .GreaterThan(0)
            .When(x => x.TimeoutHours.HasValue)
            .WithMessage("Timeout hours must be greater than zero if specified")
            .LessThanOrEqualTo(8760) // 365 days
            .When(x => x.TimeoutHours.HasValue)
            .WithMessage("Timeout hours cannot exceed 8760 (1 year)");

        RuleFor(x => x.Conditions)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Conditions))
            .WithMessage("Conditions must not exceed 4000 characters");
    }
}
