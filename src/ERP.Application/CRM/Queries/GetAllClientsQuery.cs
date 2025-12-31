using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.DTOs;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Query to get all clients.
/// </summary>
public class GetAllClientsQuery : IQuery<IEnumerable<ClientDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
