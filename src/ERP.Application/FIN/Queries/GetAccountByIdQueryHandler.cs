using ERP.Application.Common.Exceptions;
using ERP.Application.FIN.DTOs;
using ERP.Domain.FIN.Repositories;
using MediatR;

namespace ERP.Application.FIN.Queries;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByIdQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException($"Account with ID {request.AccountId} not found");
        }

        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber.Value,
            Name = account.Name,
            Description = account.Description,
            Type = account.Type.ToString(),
            Status = account.Status.ToString(),
            ParentAccountId = account.ParentAccountId,
            Balance = account.Balance,
            Currency = account.Currency,
            AllowPosting = account.AllowPosting,
            RequiresReconciliation = account.RequiresReconciliation,
            CreatedDate = account.CreatedDate,
            ModifiedDate = account.ModifiedDate
        };
    }
}
