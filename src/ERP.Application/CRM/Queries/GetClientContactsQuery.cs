using ERP.Application.CRM.DTOs;
using MediatR;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Query to get all contacts for a client.
/// </summary>
public class GetClientContactsQuery : IRequest<IEnumerable<ContactDto>>
{
    public long ClientId { get; set; }
}
