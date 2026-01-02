using ERP.Application.Common.Interfaces;
using ERP.Domain.CRM.Enums;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Command to create a new contact for a client.
/// </summary>
public class CreateContactCommand : ICommand<long>
{
    public long ClientId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public string Email { get; init; } = string.Empty;
    public ContactType ContactType { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public string? PhoneNumber { get; init; }
    public string? MobileNumber { get; init; }
    public bool IsPrimary { get; init; }
    public string? Notes { get; init; }
}
