using FluentValidation;

namespace ERP.Application.AUDIT.Commands;

/// <summary>
/// Validator for ExportUserDataCommand (GDPR Right to Data Portability).
/// </summary>
public class ExportUserDataCommandValidator : AbstractValidator<ExportUserDataCommand>
{
    public ExportUserDataCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than zero");
    }
}
