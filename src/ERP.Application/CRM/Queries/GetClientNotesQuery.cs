using ERP.Application.CRM.DTOs;
using MediatR;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Query to get all notes for a client.
/// </summary>
public class GetClientNotesQuery : IRequest<IEnumerable<NoteDto>>
{
    public long ClientId { get; set; }
}
