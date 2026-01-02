using ERP.Application.Common.Interfaces;
using ERP.Application.Identity.DTOs;

namespace ERP.Application.Identity.Commands;

/// <summary>
/// Command to authenticate a user and generate JWT tokens.
/// </summary>
public class LoginCommand : ICommand<AuthenticationResponse>
{
    /// <summary>
    /// Gets or sets the username or email.
    /// </summary>
    public string UserNameOrEmail { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to remember the user (longer refresh token).
    /// </summary>
    public bool RememberMe { get; init; }
}
