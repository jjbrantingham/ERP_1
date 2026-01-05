using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Repositories;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Handler for CreateNoteCommand.
/// </summary>
public class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand, long>
{
    private readonly INoteRepository _noteRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;

    public CreateNoteCommandHandler(
        INoteRepository noteRepository,
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)
    {
        _noteRepository = noteRepository;
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<long> Handle(CreateNoteCommand command, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Verify client exists
        var client = await _clientRepository.GetByIdAsync(command.ClientId, cancellationToken);
        if (client == null)
            throw new NotFoundException("Client not found");

        // Create note
        var note = Note.Create(
            _currentTenant.TenantId,
            command.ClientId,
            command.Subject,
            command.Content,
            command.NoteType,
            command.ContactId,
            command.AuthorId,
            command.FollowUpDate
        );

        await _noteRepository.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
