using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.CRM.DTOs;
using ERP.Domain.CRM.Repositories;
using MediatR;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Handler for GetClientByIdQuery.
/// </summary>
public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto>
{
    private readonly IClientRepository _clientRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetClientByIdQueryHandler(IClientRepository clientRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _clientRepository = clientRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<ClientDto> Handle(GetClientByIdQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var client = await _clientRepository.GetByIdAsync(query.ClientId, cancellationToken);
        if (client == null)
            throw new NotFoundException("Client not found");

        return new ClientDto
        {
            Id = client.Id,
            ClientNumber = client.ClientNumber,
            ClientType = client.ClientType.ToString(),
            Status = client.Status.ToString(),
            Name = client.Name,
            LegalName = client.LegalName,
            TaxId = client.TaxId,
            Website = client.Website,
            Industry = client.Industry,
            PrimaryEmail = client.PrimaryEmail?.Value,
            PrimaryPhone = client.PrimaryPhone,
            BillingStreet = client.BillingAddress?.Street,
            BillingStreet2 = client.BillingAddress?.Street2,
            BillingCity = client.BillingAddress?.City,
            BillingStateProvince = client.BillingAddress?.StateProvince,
            BillingPostalCode = client.BillingAddress?.PostalCode,
            BillingCountry = client.BillingAddress?.Country,
            ShippingStreet = client.ShippingAddress?.Street,
            ShippingStreet2 = client.ShippingAddress?.Street2,
            ShippingCity = client.ShippingAddress?.City,
            ShippingStateProvince = client.ShippingAddress?.StateProvince,
            ShippingPostalCode = client.ShippingAddress?.PostalCode,
            ShippingCountry = client.ShippingAddress?.Country,
            FirstContactDate = client.FirstContactDate,
            LastContactDate = client.LastContactDate,
            AccountManagerId = client.AccountManagerId,
            PaymentTerms = client.PaymentTerms,
            CreditLimit = client.CreditLimit,
            Notes = client.GeneralNotes,
            IsActive = client.IsActive,
            CreatedDate = client.CreatedDate,
            ModifiedDate = client.ModifiedDate
        };
    }
}
