using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Repositories;
using ERP.Domain.CRM.ValueObjects;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Handler for CreateClientCommand.
/// </summary>
public class CreateClientCommandHandler : ICommandHandler<CreateClientCommand, long>
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateClientCommandHandler(
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateClientCommand command, CancellationToken cancellationToken = default)
    {
        // Generate client number
        var clientNumber = Client.GenerateClientNumber();

        // Create email if provided
        Email? primaryEmail = null;
        if (!string.IsNullOrWhiteSpace(command.PrimaryEmail))
        {
            primaryEmail = new Email(command.PrimaryEmail);
        }

        // Create billing address if provided
        Address? billingAddress = null;
        if (!string.IsNullOrWhiteSpace(command.BillingStreet) &&
            !string.IsNullOrWhiteSpace(command.BillingCity) &&
            !string.IsNullOrWhiteSpace(command.BillingCountry))
        {
            billingAddress = new Address(
                command.BillingStreet,
                command.BillingCity,
                command.BillingCountry,
                command.BillingStreet2,
                command.BillingStateProvince,
                command.BillingPostalCode
            );
        }

        // Create shipping address if provided
        Address? shippingAddress = null;
        if (!string.IsNullOrWhiteSpace(command.ShippingStreet) &&
            !string.IsNullOrWhiteSpace(command.ShippingCity) &&
            !string.IsNullOrWhiteSpace(command.ShippingCountry))
        {
            shippingAddress = new Address(
                command.ShippingStreet,
                command.ShippingCity,
                command.ShippingCountry,
                command.ShippingStreet2,
                command.ShippingStateProvince,
                command.ShippingPostalCode
            );
        }

        // Create client
        var client = Client.Create(
            _currentTenant.TenantId,
            clientNumber,
            command.Name,
            command.ClientType,
            command.LegalName,
            command.TaxId,
            command.Website,
            command.Industry,
            primaryEmail,
            command.PrimaryPhone,
            billingAddress,
            shippingAddress,
            command.AccountManagerId,
            command.PaymentTerms,
            command.CreditLimit,
            command.Notes
        );

        await _clientRepository.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return client.Id;
    }
}
