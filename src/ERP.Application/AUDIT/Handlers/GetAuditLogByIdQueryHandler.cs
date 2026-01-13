using ERP.Application.AUDIT.DTOs;
using ERP.Application.AUDIT.Queries;
using ERP.Application.Common.Exceptions;
using ERP.Domain.AUDIT.Repositories;
using MediatR;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for getting a specific audit log entry by ID.
/// </summary>
public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, AuditLogDto>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetAuditLogByIdQueryHandler(IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _auditLogRepository = auditLogRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<AuditLogDto> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var auditLog = await _auditLogRepository.GetByIdAsync(request.AuditLogId, cancellationToken);

        if (auditLog == null)
            throw new NotFoundException("Audit log not found");

        return new AuditLogDto
        {
            Id = auditLog.Id,
            TenantId = auditLog.TenantId,
            EventType = auditLog.EventType,
            EventTypeName = auditLog.EventType.ToString(),
            Severity = auditLog.Severity,
            SeverityName = auditLog.Severity.ToString(),
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            UserId = auditLog.UserId,
            Username = auditLog.Username,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent,
            Description = auditLog.Description,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues,
            Metadata = auditLog.Metadata,
            Timestamp = auditLog.Timestamp
        };
    }
}
