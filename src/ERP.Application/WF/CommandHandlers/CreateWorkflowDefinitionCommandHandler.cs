using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.WF.Commands;
using ERP.Domain.WF.Entities;
using ERP.Domain.WF.Enums;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.CommandHandlers;

public class CreateWorkflowDefinitionCommandHandler : IRequestHandler<CreateWorkflowDefinitionCommand, long>
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;

    public CreateWorkflowDefinitionCommandHandler(
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<long> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Create workflow definition
        var workflow = WorkflowDefinition.Create(
            _currentTenant.TenantId,
            request.Name,
            request.Description,
            request.EntityType);

        // Add steps
        foreach (var stepCommand in request.Steps.OrderBy(s => s.SequenceNumber))
        {
            workflow.AddStep(
                stepCommand.Name,
                stepCommand.Description,
                (StepType)stepCommand.StepType,
                stepCommand.SequenceNumber,
                stepCommand.ApproverRole,
                stepCommand.ApproverId,
                stepCommand.TimeoutHours);
        }

        await _workflowDefinitionRepository.AddAsync(workflow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workflow.Id;
    }
}
