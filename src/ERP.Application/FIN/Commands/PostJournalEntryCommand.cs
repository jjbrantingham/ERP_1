using MediatR;

namespace ERP.Application.FIN.Commands;

public class PostJournalEntryCommand : IRequest
{
    public long JournalEntryId { get; set; }
}
