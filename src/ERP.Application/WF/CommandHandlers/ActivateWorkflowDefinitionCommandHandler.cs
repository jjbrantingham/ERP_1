using ERP.Application.Common.Interfaces;
using ERP.Application.WF.Commands;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.CommandHandlers;

public class ActivateWorkflowDefinitionCommandHandler : IRequestHandler<ActivateWorkflowDefinitionCommand>
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateWorkflowDefinitionCommandHandler(
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IUnitOfWork unitOfWork)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _workflowDefinitionRepository.GetByIdWithStepsAsync(
            request.WorkflowDefinitionId,
            cancellationToken);

        if (workflow == null)
            throw new InvalidOperationException("Workflow definition not found");

        workflow.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
