using ERP.Domain.Common;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.CRM.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Note entity.
/// </summary>
public class NoteRepository : Repository<Note>, INoteRepository
{
    public NoteRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Note>> GetByClientIdAsync(long clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.ClientId == clientId)
            .OrderByDescending(n => n.NoteDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetByContactIdAsync(long contactId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.ContactId == contactId)
            .OrderByDescending(n => n.NoteDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetByTypeForClientAsync(long clientId, NoteType type, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.ClientId == clientId && n.NoteType == type)
            .OrderByDescending(n => n.NoteDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetByAuthorAsync(long authorId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.AuthorId == authorId)
            .OrderByDescending(n => n.NoteDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetPendingFollowUpsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Notes
            .Where(n => n.FollowUpDate.HasValue &&
                       !n.IsFollowUpComplete &&
                       n.FollowUpDate.Value <= today)
            .OrderBy(n => n.FollowUpDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetFollowUpsDueByAsync(DateTime dueDate, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.FollowUpDate.HasValue &&
                       !n.IsFollowUpComplete &&
                       n.FollowUpDate.Value <= dueDate)
            .OrderBy(n => n.FollowUpDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetRecentNotesForClientAsync(long clientId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.ClientId == clientId)
            .OrderByDescending(n => n.NoteDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
