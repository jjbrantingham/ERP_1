using ERP.Application.FIN.DTOs;
using MediatR;

namespace ERP.Application.FIN.Queries;

public class GetAccountByIdQuery : IRequest<AccountDto>
{
    public long AccountId { get; set; }
}
