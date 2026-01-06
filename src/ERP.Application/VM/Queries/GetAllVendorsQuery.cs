using ERP.Application.Common.Interfaces;
using ERP.Application.VM.DTOs;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Query to get all vendors.
/// </summary>
public class GetAllVendorsQuery : IQuery<IEnumerable<VendorDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
