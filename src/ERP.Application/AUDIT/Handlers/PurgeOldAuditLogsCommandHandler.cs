using ERP.Application.AUDIT.Commands;
using ERP.Domain.AUDIT.Repositories;
using ERP.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for purging old audit logs (data retention).
/// </summary>
public class PurgeOldAuditLogsCommandHandler : IRequestHandler<PurgeOldAuditLogsCommand, int>
{
    private readonly ERPDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PurgeOldAuditLogsCommandHandler(
        ERPDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(PurgeOldAuditLogsCommand request, CancellationToken cancellationToken)
    {
        var logsToDelete = await _context.AuditLogs
            .Where(a => a.Timestamp < request.OlderThan)
            .ToListAsync(cancellationToken);

        var count = logsToDelete.Count;

        if (!request.DryRun && count > 0)
        {
            _context.AuditLogs.RemoveRange(logsToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return count;
    }
}
