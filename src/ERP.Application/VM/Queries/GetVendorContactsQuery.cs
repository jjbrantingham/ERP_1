using ERP.Application.Common.Interfaces;
using ERP.Application.VM.DTOs;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Query to get vendor contacts.
/// </summary>
public class GetVendorContactsQuery : IQuery<IEnumerable<VendorContactDto>>
{
    public long VendorId { get; set; }
}
