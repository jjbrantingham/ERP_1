using ERP.Domain.Common;
using ERP.Domain.AUDIT.Enums;

namespace ERP.Domain.AUDIT.Entities;

/// <summary>
/// Audit log entry for tracking all system changes and activities
/// </summary>
public class AuditLog : Entity
{
    /// <summary>
    /// Type of audit event
    /// </summary>
    public AuditEventType EventType { get; private set; }

    /// <summary>
    /// Severity level
    /// </summary>
    public AuditSeverity Severity { get; private set; }

    /// <summary>
    /// Entity type being audited (e.g., "Invoice", "Timesheet")
    /// </summary>
    public string EntityType { get; private set; }

    /// <summary>
    /// ID of the entity being audited
    /// </summary>
    public string EntityId { get; private set; }

    /// <summary>
    /// User who performed the action
    /// </summary>
    public long? UserId { get; private set; }

    /// <summary>
    /// Username who performed the action
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// IP address of the user
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User agent (browser/application)
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Description of the action
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Old values (JSON) - for Update events
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// New values (JSON) - for Create/Update events
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Timestamp of the event
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Table/module affected
    /// </summary>
    public string? TableName { get; private set; }

    /// <summary>
    /// Primary key value(s)
    /// </summary>
    public string? PrimaryKey { get; private set; }

    private AuditLog()
    {
        EntityType = null!;
        EntityId = null!;
        Username = null!;
        Description = null!;
    }

    /// <summary>
    /// Create a new audit log entry
    /// </summary>
    public static AuditLog Create(
        Guid tenantId,
        AuditEventType eventType,
        string entityType,
        string entityId,
        long? userId,
        string username,
        string description,
        AuditSeverity severity = AuditSeverity.Information,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? metadata = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            Username = username,
            Description = description,
            Severity = severity,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create an entity change audit log
    /// </summary>
    public static AuditLog CreateEntityChange(
        Guid tenantId,
        AuditEventType eventType,
        string entityType,
        long entityId,
        long? userId,
        string username,
        string tableName,
        string? oldValues = null,
        string? newValues = null)
    {
        var description = eventType switch
        {
            AuditEventType.Create => $"Created {entityType} #{entityId}",
            AuditEventType.Update => $"Updated {entityType} #{entityId}",
            AuditEventType.Delete => $"Deleted {entityType} #{entityId}",
            _ => $"{eventType} on {entityType} #{entityId}"
        };

        return new AuditLog
        {
            TenantId = tenantId,
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            UserId = userId,
            Username = username,
            Description = description,
            Severity = AuditSeverity.Information,
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = DateTime.UtcNow,
            TableName = tableName,
            PrimaryKey = entityId.ToString()
        };
    }

    /// <summary>
    /// Create a user activity audit log
    /// </summary>
    public static AuditLog CreateUserActivity(
        Guid tenantId,
        AuditEventType eventType,
        long? userId,
        string username,
        string description,
        string? ipAddress = null,
        string? userAgent = null,
        AuditSeverity severity = AuditSeverity.Information)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            EventType = eventType,
            EntityType = "User",
            EntityId = userId?.ToString() ?? "0",
            UserId = userId,
            Username = username,
            Description = description,
            Severity = severity,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a financial audit log
    /// </summary>
    public static AuditLog CreateFinancialAudit(
        Guid tenantId,
        AuditEventType eventType,
        string entityType,
        long entityId,
        long userId,
        string username,
        decimal amount,
        string description,
        string? metadata = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            UserId = userId,
            Username = username,
            Description = description,
            Severity = AuditSeverity.Warning, // Financial events are important
            Metadata = metadata ?? $"{{\"Amount\": {amount}}}",
            Timestamp = DateTime.UtcNow
        };
    }
}
