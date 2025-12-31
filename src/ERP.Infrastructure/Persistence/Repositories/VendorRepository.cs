using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Enums;
using ERP.Domain.VM.Repositories;
using ERP.Domain.VM.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Vendor aggregate root.
/// </summary>
public class VendorRepository : Repository<Vendor>, IVendorRepository
{
    public VendorRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Vendor?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Vendor?> GetByVendorNumberAsync(VendorNumber vendorNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .FirstOrDefaultAsync(v => v.VendorNumber == vendorNumber, cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> GetByStatusAsync(VendorStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .Where(v => v.Status == status)
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> GetActiveVendorsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .Where(v => v.IsActive && v.Status == VendorStatus.Active)
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> GetByTypeAsync(VendorType type, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .Where(v => v.VendorType == type)
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();

        return await _context.Vendors
            .Where(v =>
                v.Name.ToLower().Contains(normalizedSearchTerm) ||
                v.VendorNumber.Value.ToLower().Contains(normalizedSearchTerm))
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(VendorStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .CountAsync(v => v.Status == status, cancellationToken);
    }

    public async Task<bool> ExistsAsync(VendorNumber vendorNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .AnyAsync(v => v.VendorNumber == vendorNumber, cancellationToken);
    }

    public async Task<Vendor?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .FirstOrDefaultAsync(v => v.TaxId == taxId, cancellationToken);
    }
}
