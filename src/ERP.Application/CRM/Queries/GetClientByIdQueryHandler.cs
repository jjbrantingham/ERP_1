using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.DTOs;
using ERP.Domain.CRM.Repositories;

namespace ERP.Application.CRM.Queries;

/// <summary>
/// Handler for GetClientByIdQuery.
/// </summary>
public class GetClientByIdQueryHandler : IQueryHandler<GetClientByIdQuery, ClientDto>
{
    private readonly IClientRepository _clientRepository;

    public GetClientByIdQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientDto> Handle(GetClientByIdQuery query, CancellationToken cancellationToken = default)
    {
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
            Notes = client.Notes,
            IsActive = client.IsActive,
            CreatedDate = client.CreatedDate,
            ModifiedDate = client.ModifiedDate
        };
    }
}
