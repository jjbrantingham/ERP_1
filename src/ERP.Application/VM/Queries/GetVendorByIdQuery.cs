using ERP.Application.Common.Interfaces;
using ERP.Application.VM.DTOs;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Query to get vendor by ID.
/// </summary>
public class GetVendorByIdQuery : IQuery<VendorDto>
{
    public long VendorId { get; set; }
}
