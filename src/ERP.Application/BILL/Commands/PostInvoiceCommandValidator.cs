using FluentValidation;

namespace ERP.Application.BILL.Commands;

public class PostInvoiceCommandValidator : AbstractValidator<PostInvoiceCommand>
{
    public PostInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0)
            .WithMessage("Invoice ID must be greater than zero");
    }
}
