using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.WF.Commands;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.CommandHandlers;

public class DeactivateWorkflowDefinitionCommandHandler : IRequestHandler<DeactivateWorkflowDefinitionCommand>
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateWorkflowDefinitionCommandHandler(
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeactivateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var workflow = await _workflowDefinitionRepository.GetByIdAsync(
            request.WorkflowDefinitionId,
            cancellationToken);

        if (workflow == null)
            throw new InvalidOperationException("Workflow definition not found");

        workflow.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
