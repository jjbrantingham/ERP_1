using ERP.Application.Common.Interfaces;
using ERP.Application.WF.Commands;
using ERP.Domain.WF.Enums;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.CommandHandlers;

public class RejectStepCommandHandler : IRequestHandler<RejectStepCommand>
{
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RejectStepCommandHandler(
        IWorkflowInstanceRepository workflowInstanceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _workflowInstanceRepository = workflowInstanceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(RejectStepCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException("User must be authenticated");

        var workflowInstance = await _workflowInstanceRepository.GetByIdWithStepsAsync(
            request.WorkflowInstanceId,
            cancellationToken);

        if (workflowInstance == null)
            throw new InvalidOperationException("Workflow instance not found");

        // Process rejection action
        workflowInstance.ProcessStepAction(
            request.StepSequenceNumber,
            ApprovalAction.Rejected,
            _currentUser.UserId.Value,
            _currentUser.Username ?? "Unknown",
            request.Comments);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
