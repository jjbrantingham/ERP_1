using ERP.Application.Common.Interfaces;
using ERP.Domain.AUDIT.Entities;
using ERP.Domain.AUDIT.Enums;
using ERP.Domain.AUDIT.Repositories;
using ERP.Domain.Common;
using ERP.Domain.Identity.Events;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ERP.Application.AUDIT.EventHandlers;

/// <summary>
/// Event handler for successful user login - creates audit log.
/// </summary>
public class UserLoggedInAuditEventHandler : INotificationHandler<UserLoggedInEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserLoggedInAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

        var auditLog = AuditLog.CreateUserActivity(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.Login,
            userId: notification.UserId,
            username: notification.Username,
            description: $"User '{notification.Username}' logged in successfully",
            severity: AuditSeverity.Information,
            ipAddress: ipAddress,
            userAgent: userAgent
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Event handler for user logout - creates audit log.
/// </summary>
public class UserLoggedOutAuditEventHandler : INotificationHandler<UserLoggedOutEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserLoggedOutAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(UserLoggedOutEvent notification, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

        var auditLog = AuditLog.CreateUserActivity(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.Logout,
            userId: notification.UserId,
            username: notification.Username,
            description: $"User '{notification.Username}' logged out",
            severity: AuditSeverity.Information,
            ipAddress: ipAddress,
            userAgent: userAgent
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Event handler for failed login attempts - creates audit log.
/// </summary>
public class LoginFailedAuditEventHandler : INotificationHandler<LoginFailedEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginFailedAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(LoginFailedEvent notification, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

        var auditLog = AuditLog.CreateUserActivity(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.LoginFailed,
            userId: null,
            username: notification.Username,
            description: $"Failed login attempt for user '{notification.Username}'. Reason: {notification.Reason}",
            severity: AuditSeverity.Warning,
            ipAddress: ipAddress,
            userAgent: userAgent
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Event handler for password changes - creates audit log.
/// </summary>
public class PasswordChangedAuditEventHandler : INotificationHandler<PasswordChangedEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PasswordChangedAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(PasswordChangedEvent notification, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

        var auditLog = AuditLog.CreateUserActivity(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.PasswordChanged,
            userId: notification.UserId,
            username: notification.Username,
            description: $"User '{notification.Username}' changed their password",
            severity: AuditSeverity.Information,
            ipAddress: ipAddress,
            userAgent: userAgent
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Event handler for permission changes - creates audit log.
/// </summary>
public class PermissionChangedAuditEventHandler : INotificationHandler<PermissionChangedEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public PermissionChangedAuditEventHandler(
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

    public async Task Handle(PermissionChangedEvent notification, CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.CreateUserActivity(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.PermissionChanged,
            userId: notification.TargetUserId,
            username: notification.TargetUsername,
            description: $"Permissions changed for user '{notification.TargetUsername}' by '{_currentUserService.Username}'. Change: {notification.ChangeDescription}",
            severity: AuditSeverity.Warning,
            ipAddress: null,
            userAgent: null
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
