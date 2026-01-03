using ERP.Domain.AUDIT.Enums;
using MediatR;

namespace ERP.Application.AUDIT.Commands;

/// <summary>
/// Command to manually create an audit log entry (e.g., for user activity, authentication events).
/// </summary>
public class CreateAuditLogCommand : IRequest<long>
{
    public AuditEventType EventType { get; set; }
    public AuditSeverity Severity { get; set; } = AuditSeverity.Information;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Metadata { get; set; }
}
