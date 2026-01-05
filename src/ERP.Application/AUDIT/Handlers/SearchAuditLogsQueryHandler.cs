using ERP.Application.AUDIT.DTOs;
using ERP.Application.AUDIT.Queries;
using ERP.Application.Common.Models;
using ERP.Domain.AUDIT.Repositories;
using MediatR;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for searching audit logs with filters.
/// </summary>
public class SearchAuditLogsQueryHandler : IRequestHandler<SearchAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public SearchAuditLogsQueryHandler(IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _auditLogRepository = auditLogRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(SearchAuditLogsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var auditLogs = await _auditLogRepository.SearchAsync(
            entityType: request.EntityType,
            eventType: request.EventType,
            userId: request.UserId,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken
        );

        var items = auditLogs.Select(a => new AuditLogDto
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

        // Get total count
        var totalCount = await _auditLogRepository.CountAsync(
            entityType: request.EntityType,
            eventType: request.EventType,
            userId: request.UserId,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            cancellationToken: cancellationToken
        );

        return new PagedResult<AuditLogDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
