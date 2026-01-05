using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.WF.Commands;
using ERP.Domain.WF.Enums;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.CommandHandlers;

public class ApproveStepCommandHandler : IRequestHandler<ApproveStepCommand>
{
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ApproveStepCommandHandler(
        IWorkflowInstanceRepository workflowInstanceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _workflowInstanceRepository = workflowInstanceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveStepCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException("User must be authenticated");

        var workflowInstance = await _workflowInstanceRepository.GetByIdWithStepsAsync(
            request.WorkflowInstanceId,
            cancellationToken);

        if (workflowInstance == null)
            throw new InvalidOperationException("Workflow instance not found");

        // Process approval action
        workflowInstance.ProcessStepAction(
            request.StepSequenceNumber,
            ApprovalAction.Approved,
            _currentUser.UserId.Value,
            _currentUser.Username ?? "Unknown",
            request.Comments);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
