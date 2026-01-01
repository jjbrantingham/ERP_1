using ERP.Application.FIN.DTOs;
using MediatR;

namespace ERP.Application.FIN.Queries;

public class GetJournalEntryByIdQuery : IRequest<JournalEntryDto>
{
    public long JournalEntryId { get; set; }
}
