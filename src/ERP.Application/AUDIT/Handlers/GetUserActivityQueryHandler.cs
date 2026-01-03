using ERP.Application.AUDIT.DTOs;
using ERP.Application.AUDIT.Queries;
using ERP.Application.Common.Models;
using ERP.Domain.AUDIT.Repositories;
using MediatR;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for getting activity logs for a specific user.
/// </summary>
public class GetUserActivityQueryHandler : IRequestHandler<GetUserActivityQuery, PagedResult<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetUserActivityQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetUserActivityQuery request, CancellationToken cancellationToken)
    {
        var auditLogs = await _auditLogRepository.SearchAsync(
            entityType: null,
            eventType: null,
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

        var totalCount = await _auditLogRepository.CountAsync(
            entityType: null,
            eventType: null,
            userId: request.UserId,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            cancellationToken: cancellationToken
        );

        return new PagedResult<AuditLogDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
