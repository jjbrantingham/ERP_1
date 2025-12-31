using ERP.Domain.Common.Interfaces;
using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Enums;
using ERP.Domain.VM.ValueObjects;

namespace ERP.Domain.VM.Repositories;

/// <summary>
/// Repository interface for Vendor aggregate root.
/// </summary>
public interface IVendorRepository : IRepository<Vendor>
{
    /// <summary>
    /// Get vendor by ID.
    /// </summary>
    Task<Vendor?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get vendor by vendor number.
    /// </summary>
    Task<Vendor?> GetByVendorNumberAsync(VendorNumber vendorNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all vendors by status.
    /// </summary>
    Task<IEnumerable<Vendor>> GetByStatusAsync(VendorStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active vendors.
    /// </summary>
    Task<IEnumerable<Vendor>> GetActiveVendorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all vendors by type.
    /// </summary>
    Task<IEnumerable<Vendor>> GetByTypeAsync(VendorType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search vendors by name or vendor number.
    /// </summary>
    Task<IEnumerable<Vendor>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get vendor count by status.
    /// </summary>
    Task<int> GetCountByStatusAsync(VendorStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if vendor number exists.
    /// </summary>
    Task<bool> ExistsAsync(VendorNumber vendorNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get vendor by tax ID.
    /// </summary>
    Task<Vendor?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);
}
