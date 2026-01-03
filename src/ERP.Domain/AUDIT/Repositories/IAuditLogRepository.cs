using ERP.Domain.Common;
using ERP.Domain.AUDIT.Entities;
using ERP.Domain.AUDIT.Enums;

namespace ERP.Domain.AUDIT.Repositories;

/// <summary>
/// Repository for audit logs
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Get audit log by ID
    /// </summary>
    Task<AuditLog?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs by user
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByUserAsync(
        long userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs by event type
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEventTypeAsync(
        AuditEventType eventType,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs by severity
    /// </summary>
    Task<IEnumerable<AuditLog>> GetBySeverityAsync(
        AuditSeverity severity,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs by date range
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent audit logs
    /// </summary>
    Task<IEnumerable<AuditLog>> GetRecentAsync(
        int count = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search audit logs
    /// </summary>
    Task<IEnumerable<AuditLog>> SearchAsync(
        string? entityType = null,
        AuditEventType? eventType = null,
        long? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count audit logs matching the search criteria
    /// </summary>
    Task<int> CountAsync(
        string? entityType = null,
        AuditEventType? eventType = null,
        long? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}
