using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.WF.Commands;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.CommandHandlers;

public class CancelWorkflowCommandHandler : IRequestHandler<CancelWorkflowCommand>
{
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelWorkflowCommandHandler(
        IWorkflowInstanceRepository workflowInstanceRepository,
        IUnitOfWork unitOfWork)
    {
        _workflowInstanceRepository = workflowInstanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var workflowInstance = await _workflowInstanceRepository.GetByIdAsync(
            request.WorkflowInstanceId,
            cancellationToken);

        if (workflowInstance == null)
            throw new InvalidOperationException("Workflow instance not found");

        workflowInstance.Cancel(request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
