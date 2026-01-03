using ERP.Application.AUDIT.DTOs;
using MediatR;

namespace ERP.Application.AUDIT.Queries;

/// <summary>
/// Query to get a specific audit log entry by ID.
/// </summary>
public class GetAuditLogByIdQuery : IRequest<AuditLogDto>
{
    public long AuditLogId { get; set; }
}
