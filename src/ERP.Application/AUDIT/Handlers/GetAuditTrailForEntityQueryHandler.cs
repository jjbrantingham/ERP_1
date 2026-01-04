using ERP.Application.AUDIT.DTOs;
using ERP.Application.AUDIT.Queries;
using ERP.Domain.AUDIT.Repositories;
using MediatR;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for getting the complete audit trail for a specific entity.
/// </summary>
public class GetAuditTrailForEntityQueryHandler : IRequestHandler<GetAuditTrailForEntityQuery, IEnumerable<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetAuditTrailForEntityQueryHandler(IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _auditLogRepository = auditLogRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<AuditLogDto>> Handle(GetAuditTrailForEntityQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var auditLogs = await _auditLogRepository.GetByEntityAsync(
            request.EntityType,
            request.EntityId.ToString(),
            cancellationToken
        );

        return auditLogs.Select(a => new AuditLogDto
        {
            Id = a.Id,
            TenantId = a.TenantId,
            EventType = a.EventType,
            EventTypeName = a.EventType.ToString(),
            Severity = a.Severity,
            SeverityName = a.Severity.ToString(),
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            UserId = a.UserId,
            Username = a.Username,
            IpAddress = a.IpAddress,
            UserAgent = a.UserAgent,
            Description = a.Description,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            Metadata = a.Metadata,
            Timestamp = a.Timestamp
        }).ToList();
    }
}
