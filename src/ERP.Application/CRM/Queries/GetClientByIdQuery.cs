using ERP.Application.CRM.DTOs;
using MediatR;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Query to get a client by ID.
/// </summary>
public class GetClientByIdQuery : IRequest<ClientDto>
{
    public long ClientId { get; set; }
}
