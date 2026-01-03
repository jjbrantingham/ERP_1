using ERP.Application.AUDIT.Services;
using ERP.Domain.AUDIT.Enums;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Service for managing data retention policies.
/// Implements retention rules for audit logs based on event type.
/// </summary>
public class DataRetentionPolicyService : IDataRetentionPolicyService
{
    private readonly ERPDbContext _context;
    private readonly ILogger<DataRetentionPolicyService> _logger;

    // Retention periods in days by event type
    private readonly Dictionary<AuditEventType, int> _retentionPolicies = new()
    {
        // Financial records: 7 years (2555 days) for compliance
        { AuditEventType.FinancialPost, 2555 },
        { AuditEventType.FinancialReverse, 2555 },
        { AuditEventType.InvoicePosted, 2555 },
        { AuditEventType.InvoiceVoided, 2555 },
        { AuditEventType.PaymentReceived, 2555 },

        // Security events: 2 years (730 days)
        { AuditEventType.Login, 730 },
        { AuditEventType.Logout, 730 },
        { AuditEventType.LoginFailed, 730 },
        { AuditEventType.PasswordChanged, 730 },
        { AuditEventType.PermissionGranted, 730 },
        { AuditEventType.PermissionRevoked, 730 },
        { AuditEventType.PermissionChanged, 730 },

        // Workflow events: 3 years (1095 days)
        { AuditEventType.WorkflowApproved, 1095 },
        { AuditEventType.WorkflowRejected, 1095 },

        // GDPR events: Never delete (permanent retention)
        { AuditEventType.DataExported, int.MaxValue },
        { AuditEventType.DataAnonymized, int.MaxValue },

        // General CRUD operations: 1 year (365 days)
        { AuditEventType.Create, 365 },
        { AuditEventType.Update, 365 },
        { AuditEventType.Delete, 365 },

        // Configuration changes: 2 years (730 days)
        { AuditEventType.ConfigurationChanged, 730 },

        // System events: 90 days
        { AuditEventType.SystemEvent, 90 }
    };

    public DataRetentionPolicyService(
        ERPDbContext context,
        ILogger<DataRetentionPolicyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public int GetRetentionPeriodDays(AuditEventType eventType)
    {
        return _retentionPolicies.TryGetValue(eventType, out var days)
            ? days
            : 365; // Default: 1 year
    }

    public DateTime GetPurgeDate(AuditEventType eventType)
    {
        var retentionDays = GetRetentionPeriodDays(eventType);

        if (retentionDays == int.MaxValue)
            return DateTime.MinValue; // Never purge

        return DateTime.UtcNow.AddDays(-retentionDays);
    }

    public async Task<int> ApplyRetentionPoliciesAsync(CancellationToken cancellationToken = default)
    {
        var totalPurged = 0;

        try
        {
            _logger.LogInformation("Starting audit log retention policy application");

            foreach (var policy in _retentionPolicies)
            {
                var eventType = policy.Key;
                var retentionDays = policy.Value;

                // Skip permanent retention policies
                if (retentionDays == int.MaxValue)
                {
                    _logger.LogDebug(
                        "Skipping {EventType} - permanent retention policy",
                        eventType);
                    continue;
                }

                var purgeDate = GetPurgeDate(eventType);

                _logger.LogDebug(
                    "Purging {EventType} audit logs older than {PurgeDate} (retention: {RetentionDays} days)",
                    eventType, purgeDate, retentionDays);

                var logsToDelete = await _context.AuditLogs
                    .Where(a => a.EventType == eventType && a.Timestamp < purgeDate)
                    .ToListAsync(cancellationToken);

                if (logsToDelete.Any())
                {
                    _context.AuditLogs.RemoveRange(logsToDelete);
                    var deleted = logsToDelete.Count;
                    totalPurged += deleted;

                    _logger.LogInformation(
                        "Purged {Count} {EventType} audit logs older than {PurgeDate}",
                        deleted, eventType, purgeDate);
                }
            }

            if (totalPurged > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Completed audit log retention policy application. Total purged: {TotalPurged}",
                    totalPurged);
            }
            else
            {
                _logger.LogInformation("No audit logs required purging");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying audit log retention policies");
            throw;
        }

        return totalPurged;
    }
}
