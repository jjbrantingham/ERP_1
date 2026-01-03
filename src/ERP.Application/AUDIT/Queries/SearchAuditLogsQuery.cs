using ERP.Application.AUDIT.DTOs;
using ERP.Application.Common.Models;
using ERP.Domain.AUDIT.Enums;
using MediatR;

namespace ERP.Application.AUDIT.Queries;

/// <summary>
/// Query to search audit logs with various filters.
/// </summary>
public class SearchAuditLogsQuery : IRequest<PagedResult<AuditLogDto>>
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public AuditEventType? EventType { get; set; }
    public AuditSeverity? Severity { get; set; }
    public long? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
