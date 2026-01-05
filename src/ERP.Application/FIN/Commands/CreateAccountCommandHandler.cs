using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using ERP.Domain.FIN.ValueObjects;
using MediatR;

namespace ERP.Application.FIN.Commands;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, long>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<long> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);
        // Validate account number is unique
        var accountNumber = new AccountNumber(request.AccountNumber);
        if (await _accountRepository.ExistsAsync(accountNumber, cancellationToken))
        {
            throw new InvalidOperationException($"Account with number '{request.AccountNumber}' already exists");
        }

        // Parse account type
        if (!Enum.TryParse<AccountType>(request.Type, true, out var accountType))
        {
            throw new ArgumentException($"Invalid account type: {request.Type}");
        }

        // Create account
        var account = Account.Create(
            _currentTenant.TenantId,
            accountNumber,
            request.Name,
            accountType,
            request.Currency,
            request.AllowPosting,
            request.Description,
            request.ParentAccountId,
            request.RequiresReconciliation
        );

        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return account.Id;
    }
}
