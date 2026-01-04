using ERP.Application.AUDIT.Commands;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.AUDIT.Entities;
using ERP.Domain.AUDIT.Enums;
using ERP.Domain.AUDIT.Repositories;
using ERP.Domain.Common;
using ERP.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for anonymizing user data (GDPR Right to be Forgotten).
/// Anonymizes rather than deletes to maintain referential integrity and audit trails.
/// </summary>
public class AnonymizeUserDataCommandHandler : IRequestHandler<AnonymizeUserDataCommand, bool>
{
    private readonly ERPDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogRepository _auditLogRepository;

    public AnonymizeUserDataCommandHandler(
        ERPDbContext context,
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
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var anonymousId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var anonymousEmail = $"anonymized-{anonymousId}@deleted.local";
        var anonymousName = $"Anonymized User {anonymousId}";

        // Anonymize User entity
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user != null)
        {
            user.Username = anonymousEmail;
            user.Email = anonymousEmail;
            user.EmailConfirmed = false;
            user.IsActive = false;
            // Password hash is already irreversible, but we can set it to a random value
            // This would be done in the User entity's Anonymize method
        }

        // Anonymize Employee entity
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.UserId, cancellationToken);

        if (employee != null)
        {
            employee.FirstName = "Anonymized";
            employee.LastName = anonymousName;
            employee.Email = anonymousEmail;
            employee.Phone = "000-000-0000";
            employee.IsActive = false;
            employee.TerminationDate = DateTime.UtcNow;
        }

        // Anonymize personally identifiable information in audit logs
        // Note: We keep the audit log records but anonymize PII
        var userAuditLogs = await _context.AuditLogs
            .Where(a => a.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var auditLog in userAuditLogs)
        {
            auditLog.Username = anonymousName;
            auditLog.IpAddress = "0.0.0.0";
            auditLog.UserAgent = "Anonymized";
        }

        // Anonymize notes created by the user
        var notes = await _context.Notes
            .Where(n => n.CreatedBy == user!.Username)
            .ToListAsync(cancellationToken);

        foreach (var note in notes)
        {
            note.CreatedBy = anonymousName;
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
