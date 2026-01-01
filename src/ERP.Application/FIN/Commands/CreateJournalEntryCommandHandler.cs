using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using ERP.Domain.FIN.ValueObjects;
using MediatR;

namespace ERP.Application.FIN.Commands;

public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, long>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Generate unique entry number
        var entryNumber = JournalEntryNumber.Generate();
        while (await _journalEntryRepository.ExistsAsync(entryNumber, cancellationToken))
        {
            entryNumber = JournalEntryNumber.Generate();
        }

        // Parse journal entry type
        if (!Enum.TryParse<JournalEntryType>(request.Type, true, out var journalEntryType))
        {
            throw new ArgumentException($"Invalid journal entry type: {request.Type}");
        }

        // Create journal entry
        var journalEntry = JournalEntry.Create(
            _currentTenant.TenantId,
            entryNumber,
            request.EntryDate,
            request.Description,
            journalEntryType,
            request.Reference
        );

        // Add lines
        foreach (var lineCommand in request.Lines)
        {
            // Verify account exists and belongs to tenant
            var account = await _accountRepository.GetByIdAsync(lineCommand.AccountId, cancellationToken);
            if (account == null)
            {
                throw new InvalidOperationException($"Account with ID {lineCommand.AccountId} not found");
            }

            journalEntry.AddLine(
                lineCommand.AccountId,
                lineCommand.DebitAmount,
                lineCommand.CreditAmount,
                lineCommand.Description
            );
        }

        await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return journalEntry.Id;
    }
}
