using ERP.Application.Common.Interfaces;
using ERP.Application.VM.DTOs;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Query to get vendor notes.
/// </summary>
public class GetVendorNotesQuery : IQuery<IEnumerable<VendorNoteDto>>
{
    public long VendorId { get; set; }
}
