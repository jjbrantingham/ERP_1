using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.BILL.Commands;

public class PostInvoiceCommandHandler : IRequestHandler<PostInvoiceCommand>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public PostInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task Handle(PostInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Get invoice and verify tenant ownership
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        AuthorizationHelper.EnsureTenantOwnership(invoice, _currentTenant, "Invoice");

        // Post the invoice
        invoice.Post();

        // Note: InvoicePostedEvent will be raised
        // Event handler should create journal entry in FIN module:
        // DR: Accounts Receivable {ClientId}
        // CR: Revenue {ProjectId or default}

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
