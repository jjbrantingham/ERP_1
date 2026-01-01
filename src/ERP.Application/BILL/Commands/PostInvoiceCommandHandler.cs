using ERP.Application.Common.Exceptions;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.BILL.Commands;

public class PostInvoiceCommandHandler : IRequestHandler<PostInvoiceCommand>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PostInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PostInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Get invoice
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new NotFoundException($"Invoice with ID {request.InvoiceId} not found");
        }

        // Post the invoice
        invoice.Post();

        // Note: InvoicePostedEvent will be raised
        // Event handler should create journal entry in FIN module:
        // DR: Accounts Receivable {ClientId}
        // CR: Revenue {ProjectId or default}

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
