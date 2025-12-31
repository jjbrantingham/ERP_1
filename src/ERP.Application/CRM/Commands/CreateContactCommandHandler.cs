using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Repositories;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Handler for CreateContactCommand.
/// </summary>
public class CreateContactCommandHandler : ICommandHandler<CreateContactCommand, long>
{
    private readonly IContactRepository _contactRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateContactCommandHandler(
        IContactRepository contactRepository,
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _contactRepository = contactRepository;
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateContactCommand command, CancellationToken cancellationToken = default)
    {
        // Verify client exists
        var client = await _clientRepository.GetByIdAsync(command.ClientId, cancellationToken);
        if (client == null)
            throw new NotFoundException("Client not found");

        // Create contact
        var contact = Contact.Create(
            _currentTenant.TenantId,
            command.ClientId,
            command.FirstName,
            command.LastName,
            new Email(command.Email),
            command.ContactType,
            command.MiddleName,
            command.JobTitle,
            command.Department,
            command.PhoneNumber,
            command.MobileNumber,
            command.IsPrimary,
            command.Notes
        );

        await _contactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}
