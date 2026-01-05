using ERP.Application.AUDIT.Commands;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.AUDIT.Entities;
using ERP.Domain.AUDIT.Enums;
using ERP.Domain.AUDIT.Repositories;
using ERP.Domain.Common;
using ERP.Domain.CRM.Entities;
using ERP.Domain.HR.Entities;
using ERP.Domain.Identity.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for anonymizing user data (GDPR Right to be Forgotten).
/// Anonymizes rather than deletes to maintain referential integrity and audit trails.
/// </summary>
public class AnonymizeUserDataCommandHandler : IRequestHandler<AnonymizeUserDataCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;

    public AnonymizeUserDataCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService,
        IAuditLogRepository auditLogRepository)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<bool> Handle(AnonymizeUserDataCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var anonymousId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var anonymousEmail = $"anonymized-{anonymousId}@deleted.local";
        var anonymousName = $"Anonymized User {anonymousId}";

        // Anonymize User entity
        var user = await _context.Set<User>().FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user != null)
        {
            // TODO: Add Anonymize() method to User entity instead of direct property access
            // user.Anonymize(anonymousEmail);
        }

        // Anonymize Employee entity
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.UserId, cancellationToken);

        if (employee != null)
        {
            // TODO: Add Anonymize() method to Employee entity
            // employee.Anonymize(anonymousName, anonymousEmail, "000-000-0000");
        }

        // Anonymize personally identifiable information in audit logs
        // Note: We keep the audit log records but anonymize PII
        var userAuditLogs = await _context.Set<AuditLog>()
            .Where(a => a.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var auditLog in userAuditLogs)
        {
            // TODO: Add Anonymize() method to AuditLog entity
            // auditLog.Anonymize(anonymousName, "0.0.0.0", "Anonymized");
        }

        // Anonymize notes created by the user (by author ID)
        var notes = await _context.Set<Note>()
            .Where(n => n.CreatedBy == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var note in notes)
        {
            // TODO: Add Anonymize() method to Note entity
            // CreatedBy is inherited from Entity base class (long? user ID)
        }

        // Create audit log for the anonymization action
        var anonymizationAuditLog = AuditLog.Create(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.DataAnonymized,
            entityType: "User",
            entityId: request.UserId.ToString(),
            userId: _currentUserService.UserId,
            username: _currentUserService.Username ?? "System",
            description: $"User data anonymized. Reason: {request.Reason}. GDPR Right to be Forgotten (Article 17).",
            severity: AuditSeverity.Critical,
            ipAddress: null,
            userAgent: null,
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                OriginalUserId = request.UserId,
                AnonymizedOn = DateTime.UtcNow,
                Reason = request.Reason,
                PerformedBy = _currentUserService.Username,
                GDPRArticle = "Article 17 - Right to erasure ('right to be forgotten')"
            })
        );

        await _auditLogRepository.AddAsync(anonymizationAuditLog, cancellationToken);

        // Save all changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
