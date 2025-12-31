using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.DTOs;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Query to get all contacts for a client.
/// </summary>
public class GetClientContactsQuery : IQuery<IEnumerable<ContactDto>>
{
    public long ClientId { get; set; }
}
