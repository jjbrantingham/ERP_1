using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.CRM.DTOs;
using ERP.Domain.CRM.Repositories;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Handler for GetClientNotesQuery.
/// </summary>
public class GetClientNotesQueryHandler : IQueryHandler<GetClientNotesQuery, IEnumerable<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetClientNotesQueryHandler(INoteRepository noteRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _noteRepository = noteRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<NoteDto>> Handle(GetClientNotesQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

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
