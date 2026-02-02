using ERP.Domain.Common;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Enums;

namespace ERP.Domain.VM.Repositories;

/// <summary>
/// Repository interface for VendorContact entity.
/// </summary>
public interface IVendorContactRepository : IRepository<VendorContact>
{
    /// <summary>
    /// Get contact by ID.
    /// </summary>
    new Task<VendorContact?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all contacts for a vendor.
    /// </summary>
    Task<IEnumerable<VendorContact>> GetByVendorIdAsync(long vendorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get primary contact for a vendor.
    /// </summary>
    Task<VendorContact?> GetPrimaryContactAsync(long vendorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get contacts by type for a vendor.
    /// </summary>
    Task<IEnumerable<VendorContact>> GetByTypeAsync(long vendorId, ContactType contactType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search contacts by name or email.
    /// </summary>
    Task<IEnumerable<VendorContact>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
