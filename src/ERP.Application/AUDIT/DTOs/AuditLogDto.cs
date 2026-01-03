using ERP.Domain.AUDIT.Enums;

namespace ERP.Application.AUDIT.DTOs;

/// <summary>
/// DTO for audit log entries.
/// </summary>
public class AuditLogDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public AuditEventType EventType { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public AuditSeverity Severity { get; set; }
    public string SeverityName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public long? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Metadata { get; set; }
    public DateTime Timestamp { get; set; }
}
