using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.DTOs;
using ERP.Domain.CRM.Repositories;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Handler for GetClientNotesQuery.
/// </summary>
public class GetClientNotesQueryHandler : IQueryHandler<GetClientNotesQuery, IEnumerable<NoteDto>>
{
    private readonly INoteRepository _noteRepository;

    public GetClientNotesQueryHandler(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<IEnumerable<NoteDto>> Handle(GetClientNotesQuery query, CancellationToken cancellationToken = default)
    {
        var notes = await _noteRepository.GetByClientIdAsync(query.ClientId, cancellationToken);

        return notes.Select(n => new NoteDto
        {
            Id = n.Id,
            ClientId = n.ClientId,
            ContactId = n.ContactId,
            NoteType = n.NoteType.ToString(),
            Subject = n.Subject,
            Content = n.Content,
            NoteDate = n.NoteDate,
            FollowUpDate = n.FollowUpDate,
            IsFollowUpComplete = n.IsFollowUpComplete,
            AuthorId = n.AuthorId,
            CreatedDate = n.CreatedDate
        });
    }
}
