using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.DTOs;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Query to get a client by ID.
/// </summary>
public class GetClientByIdQuery : IQuery<ClientDto>
{
    public long ClientId { get; set; }
}
