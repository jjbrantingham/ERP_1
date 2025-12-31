using ERP.Application.Common.Interfaces;
using ERP.Application.VM.DTOs;
using ERP.Domain.VM.Repositories;

namespace ERP.Application.VM.Queries;

/// <summary>
/// Handler for GetVendorNotesQuery.
/// </summary>
public class GetVendorNotesQueryHandler : IQueryHandler<GetVendorNotesQuery, IEnumerable<VendorNoteDto>>
{
    private readonly IVendorNoteRepository _vendorNoteRepository;

    public GetVendorNotesQueryHandler(IVendorNoteRepository vendorNoteRepository)
    {
        _vendorNoteRepository = vendorNoteRepository;
    }

    public async Task<IEnumerable<VendorNoteDto>> Handle(GetVendorNotesQuery query, CancellationToken cancellationToken = default)
    {
        var notes = await _vendorNoteRepository.GetByVendorIdAsync(query.VendorId, cancellationToken);

        return notes.Select(n => new VendorNoteDto
        {
            Id = n.Id,
            TenantId = n.TenantId,
            VendorId = n.VendorId,
            Subject = n.Subject,
            Content = n.Content,
            NoteDate = n.NoteDate,
            CreatedByUserId = n.CreatedByUserId,
            CreatedDate = n.CreatedDate,
            ModifiedDate = n.ModifiedDate
        });
    }
}
