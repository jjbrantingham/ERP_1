using ERP.Domain.Common;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.CRM.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Contact entity.
/// </summary>
public class ContactRepository : Repository<Contact>, IContactRepository
{
    public ContactRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Contact>> GetByClientIdAsync(long clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contact?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.Email.Value == email, cancellationToken);
    }

    public async Task<Contact?> GetPrimaryContactForClientAsync(long clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsPrimary, cancellationToken);
    }

    public async Task<IEnumerable<Contact>> GetByTypeForClientAsync(long clientId, ContactType type, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .Where(c => c.ClientId == clientId && c.ContactType == type)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contact>> GetActiveContactsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .Where(c => c.IsActive)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contact>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();

        return await _context.Contacts
            .Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.Email.Value.ToLower().Contains(term))
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }
}
