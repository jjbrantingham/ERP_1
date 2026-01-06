using ERP.Domain.Common;
using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for VendorNote entity.
/// </summary>
public class VendorNoteRepository : Repository<VendorNote>, IVendorNoteRepository
{
    public VendorNoteRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<VendorNote?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.VendorNotes
            .Include(n => n.Vendor)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<VendorNote>> GetByVendorIdAsync(long vendorId, CancellationToken cancellationToken = default)
    {
        return await _context.VendorNotes
            .Where(n => n.VendorId == vendorId)
            .OrderByDescending(n => n.NoteDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VendorNote>> GetRecentNotesAsync(long vendorId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.VendorNotes
            .Where(n => n.VendorId == vendorId)
            .OrderByDescending(n => n.NoteDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VendorNote>> SearchAsync(long vendorId, string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();

        return await _context.VendorNotes
            .Where(n => n.VendorId == vendorId &&
                (n.Subject.ToLower().Contains(normalizedSearchTerm) ||
                 n.Content.ToLower().Contains(normalizedSearchTerm)))
            .OrderByDescending(n => n.NoteDate)
            .ToListAsync(cancellationToken);
    }
}
