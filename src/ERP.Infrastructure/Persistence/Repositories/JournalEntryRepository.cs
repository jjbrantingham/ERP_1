using ERP.Domain.Common;
using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using ERP.Domain.FIN.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

public class JournalEntryRepository : Repository<JournalEntry>, IJournalEntryRepository
{
    public JournalEntryRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<JournalEntry?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.Id == id, cancellationToken);
    }

    public async Task<JournalEntry?> GetByEntryNumberAsync(
        JournalEntryNumber entryNumber,
        CancellationToken cancellationToken = default)
    {
        var entryNumberValue = entryNumber.Value;
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.EntryNumber.Value == entryNumberValue, cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .OrderByDescending(je => je.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetByStatusAsync(
        JournalEntryStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .Where(je => je.Status == status)
            .OrderByDescending(je => je.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .Where(je => je.EntryDate >= startDate && je.EntryDate <= endDate)
            .OrderByDescending(je => je.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetByFiscalPeriodAsync(
        string fiscalPeriod,
        CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .Where(je => je.FiscalPeriod == fiscalPeriod)
            .OrderByDescending(je => je.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetDraftEntriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .Include(je => je.Lines)
            .Where(je => je.Status == JournalEntryStatus.Draft)
            .OrderByDescending(je => je.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        JournalEntryNumber entryNumber,
        CancellationToken cancellationToken = default)
    {
        var entryNumberValue = entryNumber.Value;
        return await _context.JournalEntries
            .AnyAsync(je => je.EntryNumber.Value == entryNumberValue, cancellationToken);
    }
}
