using ERP.Application.Common.Interfaces;
using ERP.Application.Identity.DTOs;

namespace ERP.Application.Identity.Commands;

/// <summary>
/// Command to register a new user.
/// </summary>
public class RegisterCommand : ICommand<AuthenticationResponse>
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password confirmation.
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID (for multi-tenant registration).
    /// If not provided, will be resolved from context.
    /// </summary>
    public Guid? TenantId { get; set; }
}
