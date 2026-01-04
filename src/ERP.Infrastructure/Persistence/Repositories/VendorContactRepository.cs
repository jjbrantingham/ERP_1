using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Enums;
using ERP.Domain.VM.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for VendorContact entity.
/// </summary>
public class VendorContactRepository : Repository<VendorContact>, IVendorContactRepository
{
    public VendorContactRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<VendorContact?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.VendorContacts
            .Include(c => c.Vendor)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<VendorContact>> GetByVendorIdAsync(long vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.VendorContacts
            .Where(c => c.VendorId == vendorId)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<VendorContact?> GetPrimaryContactAsync(long vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.VendorContacts
            .FirstOrDefaultAsync(c => c.VendorId == vendorId && c.IsPrimary, cancellationToken);
    }

    public async Task<IEnumerable<VendorContact>> GetByTypeAsync(long vendorId, ContactType contactType, CancellationToken cancellationToken = default)
    {
        return await _context.VendorContacts
            .Where(c => c.VendorId == vendorId && c.ContactType == contactType)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VendorContact>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();

        return await _context.VendorContacts
            .Where(c =>
                c.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                c.LastName.ToLower().Contains(normalizedSearchTerm) ||
                c.Email.Value.ToLower().Contains(normalizedSearchTerm))
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);
    }
}
