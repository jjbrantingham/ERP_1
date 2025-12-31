using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;
using ERP.Domain.CRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Client aggregate.
/// </summary>
public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Client?> GetByIdWithDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Notes)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Client?> GetByClientNumberAsync(string clientNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .FirstOrDefaultAsync(c => c.ClientNumber == clientNumber, cancellationToken);
    }

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .FirstOrDefaultAsync(c => c.PrimaryEmail!.Value == email, cancellationToken);
    }

    public async Task<IEnumerable<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .Where(c => c.IsActive && c.Status == ClientStatus.Active)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Client>> GetByStatusAsync(ClientStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .Where(c => c.Status == status)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Client>> GetByTypeAsync(ClientType type, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .Where(c => c.ClientType == type)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Client>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .Where(c => c.Industry == industry)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Client>> GetByAccountManagerAsync(long accountManagerId, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .Where(c => c.AccountManagerId == accountManagerId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Client>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();

        return await _context.Clients
            .Where(c =>
                c.ClientNumber.ToLower().Contains(term) ||
                c.Name.ToLower().Contains(term) ||
                (c.LegalName != null && c.LegalName.ToLower().Contains(term)))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
