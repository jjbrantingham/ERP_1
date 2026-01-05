using ERP.Domain.Common;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;

namespace ERP.Domain.CRM.Repositories;

/// <summary>
/// Repository interface for Contact entity.
/// </summary>
public interface IContactRepository : IRepository<Contact>
{
    /// <summary>
    /// Gets all contacts for a client.
    /// </summary>
    Task<IEnumerable<Contact>> GetByClientIdAsync(long clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a contact by email.
    /// </summary>
    Task<Contact?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the primary contact for a client.
    /// </summary>
    Task<Contact?> GetPrimaryContactForClientAsync(long clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets contacts by type for a client.
    /// </summary>
    Task<IEnumerable<Contact>> GetByTypeForClientAsync(long clientId, ContactType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active contacts.
    /// </summary>
    Task<IEnumerable<Contact>> GetActiveContactsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches contacts by name or email.
    /// </summary>
    Task<IEnumerable<Contact>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
