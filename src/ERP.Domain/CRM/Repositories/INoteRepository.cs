using ERP.Domain.Common;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;

namespace ERP.Domain.CRM.Repositories;

/// <summary>
/// Repository interface for Note entity.
/// </summary>
public interface INoteRepository : IRepository<Note>
{
    /// <summary>
    /// Gets all notes for a client.
    /// </summary>
    Task<IEnumerable<Note>> GetByClientIdAsync(long clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all notes for a contact.
    /// </summary>
    Task<IEnumerable<Note>> GetByContactIdAsync(long contactId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notes by type for a client.
    /// </summary>
    Task<IEnumerable<Note>> GetByTypeForClientAsync(long clientId, NoteType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notes by author.
    /// </summary>
    Task<IEnumerable<Note>> GetByAuthorAsync(long authorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notes with pending follow-ups.
    /// </summary>
    Task<IEnumerable<Note>> GetPendingFollowUpsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notes with follow-ups due by a specific date.
    /// </summary>
    Task<IEnumerable<Note>> GetFollowUpsDueByAsync(DateTime dueDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent notes for a client.
    /// </summary>
    Task<IEnumerable<Note>> GetRecentNotesForClientAsync(long clientId, int count = 10, CancellationToken cancellationToken = default);
}
