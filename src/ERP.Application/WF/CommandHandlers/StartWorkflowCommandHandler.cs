using ERP.Application.Common.Interfaces;
using ERP.Application.WF.Commands;
using ERP.Domain.WF.Entities;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.CommandHandlers;

public class StartWorkflowCommandHandler : IRequestHandler<StartWorkflowCommand, long>
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public StartWorkflowCommandHandler(
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IWorkflowInstanceRepository workflowInstanceRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _workflowInstanceRepository = workflowInstanceRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        // Get workflow definition with steps
        var workflowDefinition = await _workflowDefinitionRepository.GetByIdWithStepsAsync(
            request.WorkflowDefinitionId,
            cancellationToken);

        if (workflowDefinition == null)
            throw new InvalidOperationException("Workflow definition not found");

        // Verify entity type matches
        if (workflowDefinition.EntityType != request.EntityType)
            throw new InvalidOperationException(
                $"Workflow is for {workflowDefinition.EntityType}, not {request.EntityType}");

        // Check for existing workflow on this entity
        var existingWorkflow = await _workflowInstanceRepository.GetByEntityAsync(
            request.EntityType,
            request.EntityId,
            cancellationToken);

        if (existingWorkflow != null && !existingWorkflow.IsTerminal())
            throw new InvalidOperationException("An active workflow already exists for this entity");

        // Start workflow instance
        var workflowInstance = WorkflowInstance.Start(
            _currentTenant.TenantId,
            workflowDefinition.Id,
            request.EntityType,
            request.EntityId,
            workflowDefinition.Steps);

        // Begin execution
        workflowInstance.Begin();

        await _workflowInstanceRepository.AddAsync(workflowInstance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workflowInstance.Id;
    }
}
