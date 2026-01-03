using ERP.Application.AUDIT.DTOs;
using MediatR;

namespace ERP.Application.AUDIT.Queries;

/// <summary>
/// Query to get the complete audit trail for a specific entity.
/// </summary>
public class GetAuditTrailForEntityQuery : IRequest<IEnumerable<AuditLogDto>>
{
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
}
