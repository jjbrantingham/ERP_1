using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.WF.DTOs;
using ERP.Application.WF.Queries;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.QueryHandlers;

public class GetWorkflowDefinitionsByEntityTypeQueryHandler
    : IRequestHandler<GetWorkflowDefinitionsByEntityTypeQuery, IEnumerable<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetWorkflowDefinitionsByEntityTypeQueryHandler(IWorkflowDefinitionRepository workflowDefinitionRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<WorkflowDefinitionDto>> Handle(
        GetWorkflowDefinitionsByEntityTypeQuery request,
        CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var workflows = request.ActiveOnly
            ? await _workflowDefinitionRepository.GetActiveByEntityTypeAsync(request.EntityType, cancellationToken)
            : await _workflowDefinitionRepository.GetAllByEntityTypeAsync(request.EntityType, cancellationToken);

        return workflows.Select(w => new WorkflowDefinitionDto
        {
            Id = w.Id,
            TenantId = w.TenantId,
            Name = w.Name,
            Description = w.Description,
            EntityType = w.EntityType,
            Status = w.Status,
            Version = w.Version,
            CreatedDate = w.CreatedDate,
            CreatedBy = w.CreatedBy,
            Steps = w.Steps.Select(s => new WorkflowStepDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                StepType = s.StepType,
                SequenceNumber = s.SequenceNumber,
                ApproverRole = s.ApproverRole,
                ApproverId = s.ApproverId,
                TimeoutHours = s.TimeoutHours,
                Conditions = s.Conditions
            }).OrderBy(s => s.SequenceNumber).ToList()
        }).ToList();
    }
}
