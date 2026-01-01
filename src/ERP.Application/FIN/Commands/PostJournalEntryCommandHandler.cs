using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.FIN.Repositories;
using MediatR;

namespace ERP.Application.FIN.Commands;

public class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public PostJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Get journal entry
        var journalEntry = await _journalEntryRepository.GetByIdAsync(request.JournalEntryId, cancellationToken);
        if (journalEntry == null)
        {
            throw new NotFoundException($"Journal entry with ID {request.JournalEntryId} not found");
        }

        // Post the entry (validates that debits = credits)
        var userName = _currentUser.UserName ?? "System";
        journalEntry.Post(userName);

        // Apply transactions to accounts
        foreach (var line in journalEntry.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(line.AccountId, cancellationToken);
            if (account == null)
            {
                throw new NotFoundException($"Account with ID {line.AccountId} not found");
            }

            account.ApplyTransaction(line.DebitAmount, line.CreditAmount);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
