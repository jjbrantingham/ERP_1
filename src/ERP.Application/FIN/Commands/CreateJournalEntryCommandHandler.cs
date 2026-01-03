using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using ERP.Domain.FIN.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ERP.Application.FIN.Commands;

public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, long>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ILogger<CreateJournalEntryCommandHandler> _logger;

    public CreateJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ILogger<CreateJournalEntryCommandHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<long> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var totalDebits = request.Lines.Sum(l => l.DebitAmount);
        var totalCredits = request.Lines.Sum(l => l.CreditAmount);

        _logger.LogInformation(
            "Creating journal entry for TenantId: {TenantId}, Type: {Type}, Date: {EntryDate}, Lines: {LineCount}, Debits: {TotalDebits}, Credits: {TotalCredits}",
            _currentTenant.TenantId, request.Type, request.EntryDate, request.Lines.Count, totalDebits, totalCredits);

        // Generate unique entry number
        var entryNumber = JournalEntryNumber.Generate();
        while (await _journalEntryRepository.ExistsAsync(entryNumber, cancellationToken))
        {
            _logger.LogDebug("Journal entry number collision detected. Regenerating...");
            entryNumber = JournalEntryNumber.Generate();
        }

        // Parse journal entry type
        if (!Enum.TryParse<JournalEntryType>(request.Type, true, out var journalEntryType))
        {
            _logger.LogError("Invalid journal entry type provided: {Type}", request.Type);
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
                _logger.LogError("Account not found: {AccountId}", lineCommand.AccountId);
                throw new InvalidOperationException($"Account with ID {lineCommand.AccountId} not found");
            }

            journalEntry.AddLine(
                lineCommand.AccountId,
                lineCommand.DebitAmount,
                lineCommand.CreditAmount,
                lineCommand.Description
            );
        }

        // Verify journal entry is balanced before saving
        if (!journalEntry.IsBalanced())
        {
            _logger.LogError(
                "Journal entry is not balanced. Total Debits: {TotalDebits}, Total Credits: {TotalCredits}",
                totalDebits, totalCredits);
            throw new InvalidOperationException("Journal entry debits and credits must be equal");
        }

        await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully created journal entry {JournalEntryId} with number {EntryNumber}, Debits/Credits: {Amount}",
            journalEntry.Id, entryNumber.Value, totalDebits);

        return journalEntry.Id;
    }
}
