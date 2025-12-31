using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.DTOs;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Query to get all notes for a client.
/// </summary>
public class GetClientNotesQuery : IQuery<IEnumerable<NoteDto>>
{
    public long ClientId { get; set; }
}
