using ERP.Domain.Common.Interfaces;
using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.ValueObjects;

namespace ERP.Domain.FIN.Repositories;

public interface IJournalEntryRepository : IRepository<JournalEntry>
{
    Task<JournalEntry?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<JournalEntry?> GetByEntryNumberAsync(JournalEntryNumber entryNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetByFiscalPeriodAsync(string fiscalPeriod, CancellationToken cancellationToken = default);
    Task<IEnumerable<JournalEntry>> GetDraftEntriesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(JournalEntryNumber entryNumber, CancellationToken cancellationToken = default);
}
