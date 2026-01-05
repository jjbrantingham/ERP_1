using ERP.Application.AUDIT.Commands;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.AUDIT.Entities;
using ERP.Domain.AUDIT.Repositories;
using ERP.Domain.Common;
using MediatR;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for creating manual audit log entries.
/// </summary>
public class CreateAuditLogCommandHandler : IRequestHandler<CreateAuditLogCommand, long>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateAuditLogCommandHandler(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    public async Task<long> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var auditLog = AuditLog.Create(
            tenantId: _currentTenantService.TenantId,
            eventType: request.EventType,
            entityType: request.EntityType ?? string.Empty,
            entityId: request.EntityId,
            userId: _currentUserService.UserId,
            username: _currentUserService.Username ?? "System",
            description: request.Description,
            severity: request.Severity,
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent,
            metadata: request.Metadata
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return auditLog.Id;
    }
}
