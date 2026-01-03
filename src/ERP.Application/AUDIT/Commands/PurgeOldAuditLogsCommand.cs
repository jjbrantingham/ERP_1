using MediatR;

namespace ERP.Application.AUDIT.Commands;

/// <summary>
/// Command to purge audit logs older than a specified date (for data retention compliance).
/// </summary>
public class PurgeOldAuditLogsCommand : IRequest<int>
{
    public DateTime OlderThan { get; set; }
    public bool DryRun { get; set; } = false;
}
