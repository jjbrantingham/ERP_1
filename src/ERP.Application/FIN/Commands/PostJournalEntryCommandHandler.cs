using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
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
    private readonly ICurrentTenantService _currentTenant;

    public PostJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Get journal entry and verify tenant ownership
        var journalEntry = await _journalEntryRepository.GetByIdAsync(request.JournalEntryId, cancellationToken);
        AuthorizationHelper.EnsureTenantOwnership(journalEntry, _currentTenant, "Journal Entry");

        // Post the entry (validates that debits = credits)
        var userName = _currentUser.Username ?? "System";
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
