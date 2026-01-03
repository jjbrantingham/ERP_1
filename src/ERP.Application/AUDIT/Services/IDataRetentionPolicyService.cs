using ERP.Domain.AUDIT.Enums;

namespace ERP.Application.AUDIT.Services;

/// <summary>
/// Service for managing data retention policies.
/// </summary>
public interface IDataRetentionPolicyService
{
    /// <summary>
    /// Get retention period in days for a specific audit event type.
    /// </summary>
    int GetRetentionPeriodDays(AuditEventType eventType);

    /// <summary>
    /// Apply retention policies and purge old audit logs.
    /// </summary>
    Task<int> ApplyRetentionPoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the date before which audit logs should be purged.
    /// </summary>
    DateTime GetPurgeDate(AuditEventType eventType);
}
