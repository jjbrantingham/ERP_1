using ERP.Application.CRM.DTOs;
using MediatR;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Query to get all clients.
/// </summary>
public class GetAllClientsQuery : IRequest<IEnumerable<ClientDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
