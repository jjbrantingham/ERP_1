using ERP.Domain.Common;
using ERP.Domain.Common;
using ERP.Domain.CRM.Enums;
using ERP.Domain.CRM.ValueObjects;

namespace ERP.Domain.CRM.Entities;

/// <summary>
/// Represents a client (customer) in the CRM system.
/// This is the main aggregate root for the CRM module.
/// </summary>
public class Client : AggregateRoot
{
    public string ClientNumber { get; private set; }
    public ClientType ClientType { get; private set; }
    public ClientStatus Status { get; private set; }

    // Organization information (for corporate clients)
    public string Name { get; private set; }
    public string? LegalName { get; private set; }
    public string? TaxId { get; private set; }
    public string? Website { get; private set; }
    public string? Industry { get; private set; }

    // Contact information
    public Email? PrimaryEmail { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public Address? BillingAddress { get; private set; }
    public Address? ShippingAddress { get; private set; }

    // Business relationship
    public DateTime? FirstContactDate { get; private set; }
    public DateTime? LastContactDate { get; private set; }
    public long? AccountManagerId { get; private set; } // Employee who manages this client

    // Financial
    public string? PaymentTerms { get; private set; }
    public string? CreditLimit { get; private set; }

    // Additional
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<Contact> _contacts = new();
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

    private readonly List<Note> _notes = new();
    public IReadOnlyCollection<Note> Notes => _notes.AsReadOnly();

    private Client()
    {
        ClientNumber = string.Empty;
        Name = string.Empty;
    }

    /// <summary>
    /// Creates a new client.
    /// </summary>
    public static Client Create(
        Guid tenantId,
        string clientNumber,
        string name,
        ClientType clientType,
        string? legalName = null,
        string? taxId = null,
        string? website = null,
        string? industry = null,
        Email? primaryEmail = null,
        string? primaryPhone = null,
        Address? billingAddress = null,
        Address? shippingAddress = null,
        long? accountManagerId = null,
        string? paymentTerms = null,
        string? creditLimit = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(clientNumber))
            throw new ArgumentException("Client number is required", nameof(clientNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Client name is required", nameof(name));

        var client = new Client
        {
            TenantId = tenantId,
            ClientNumber = clientNumber.Trim(),
            Name = name.Trim(),
            LegalName = legalName?.Trim(),
            ClientType = clientType,
            Status = ClientStatus.Prospect,
            TaxId = taxId?.Trim(),
            Website = website?.Trim(),
            Industry = industry?.Trim(),
            PrimaryEmail = primaryEmail,
            PrimaryPhone = primaryPhone?.Trim(),
            BillingAddress = billingAddress,
            ShippingAddress = shippingAddress,
            FirstContactDate = DateTime.UtcNow,
            AccountManagerId = accountManagerId,
            PaymentTerms = paymentTerms?.Trim(),
            CreditLimit = creditLimit?.Trim(),
            Notes = notes?.Trim(),
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        client.AddDomainEvent(new ClientCreatedEvent(client.Id, client.ClientNumber, client.Name));

        return client;
    }

    /// <summary>
    /// Updates client information.
    /// </summary>
    public void UpdateInfo(
        string name,
        ClientType clientType,
        string? legalName = null,
        string? taxId = null,
        string? website = null,
        string? industry = null,
        Email? primaryEmail = null,
        string? primaryPhone = null,
        Address? billingAddress = null,
        Address? shippingAddress = null,
        long? accountManagerId = null,
        string? paymentTerms = null,
        string? creditLimit = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Client name is required", nameof(name));

        Name = name.Trim();
        LegalName = legalName?.Trim();
        ClientType = clientType;
        TaxId = taxId?.Trim();
        Website = website?.Trim();
        Industry = industry?.Trim();
        PrimaryEmail = primaryEmail;
        PrimaryPhone = primaryPhone?.Trim();
        BillingAddress = billingAddress;
        ShippingAddress = shippingAddress;
        AccountManagerId = accountManagerId;
        PaymentTerms = paymentTerms?.Trim();
        CreditLimit = creditLimit?.Trim();
        Notes = notes?.Trim();
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes the client's status.
    /// </summary>
    public void ChangeStatus(ClientStatus newStatus)
    {
        var oldStatus = Status;
        Status = newStatus;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new ClientStatusChangedEvent(Id, ClientNumber, Name, oldStatus, newStatus));
    }

    /// <summary>
    /// Activates the client.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        if (Status == ClientStatus.Inactive || Status == ClientStatus.Former)
        {
            Status = ClientStatus.Active;
        }
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the client.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        Status = ClientStatus.Inactive;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a contact interaction.
    /// </summary>
    public void RecordContact()
    {
        LastContactDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a contact to the client.
    /// </summary>
    public void AddContact(Contact contact)
    {
        if (contact.ClientId != Id)
            throw new ArgumentException("Contact does not belong to this client", nameof(contact));

        _contacts.Add(contact);
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a note to the client.
    /// </summary>
    public void AddNote(Note note)
    {
        if (note.ClientId != Id)
            throw new ArgumentException("Note does not belong to this client", nameof(note));

        _notes.Add(note);
        LastContactDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a client number.
    /// </summary>
    public static string GenerateClientNumber()
    {
        return $"CLI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..26];
    }
}
