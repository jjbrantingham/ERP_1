using ERP.Application.Common.Exceptions;
using ERP.Application.FIN.DTOs;
using ERP.Domain.FIN.Repositories;
using MediatR;

namespace ERP.Application.FIN.Queries;

public class GetJournalEntryByIdQueryHandler : IRequestHandler<GetJournalEntryByIdQuery, JournalEntryDto>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;

    public GetJournalEntryByIdQueryHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<JournalEntryDto> Handle(GetJournalEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var journalEntry = await _journalEntryRepository.GetByIdAsync(request.JournalEntryId, cancellationToken);

        if (journalEntry == null)
        {
            throw new NotFoundException($"Journal entry with ID {request.JournalEntryId} not found");
        }

        // Map lines with account details
        var lines = new List<JournalEntryLineDto>();
        foreach (var line in journalEntry.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(line.AccountId, cancellationToken);

            lines.Add(new JournalEntryLineDto
            {
                Id = line.Id,
                AccountId = line.AccountId,
                AccountNumber = account?.AccountNumber.Value ?? "Unknown",
                AccountName = account?.Name ?? "Unknown",
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                Description = line.Description
            });
        }

        return new JournalEntryDto
        {
            Id = journalEntry.Id,
            EntryNumber = journalEntry.EntryNumber.Value,
            EntryDate = journalEntry.EntryDate,
            Type = journalEntry.Type.ToString(),
            Status = journalEntry.Status.ToString(),
            Description = journalEntry.Description,
            Reference = journalEntry.Reference,
            FiscalPeriod = journalEntry.FiscalPeriod,
            TotalDebits = journalEntry.CalculateDebitTotal(),
            TotalCredits = journalEntry.CalculateCreditTotal(),
            IsBalanced = journalEntry.IsBalanced(),
            Lines = lines,
            PostedDate = journalEntry.PostedDate,
            PostedBy = journalEntry.PostedBy,
            CreatedDate = journalEntry.CreatedDate,
            ModifiedDate = journalEntry.ModifiedDate
        };
    }
}
