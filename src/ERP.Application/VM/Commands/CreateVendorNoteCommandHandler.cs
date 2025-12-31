using ERP.Application.Common.Interfaces;
using ERP.Domain.VM.Entities;
using ERP.Domain.VM.Repositories;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Handler for CreateVendorNoteCommand.
/// </summary>
public class CreateVendorNoteCommandHandler : ICommandHandler<CreateVendorNoteCommand, long>
{
    private readonly IVendorNoteRepository _vendorNoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateVendorNoteCommandHandler(
        IVendorNoteRepository vendorNoteRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _vendorNoteRepository = vendorNoteRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateVendorNoteCommand command, CancellationToken cancellationToken = default)
    {
        var note = VendorNote.Create(
            _currentTenant.TenantId,
            command.VendorId,
            command.Subject,
            command.Content,
            command.NoteDate
        );

        await _vendorNoteRepository.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
