using ERP.Application.Common.Interfaces;
using ERP.Application.Identity.DTOs;

namespace ERP.Application.Identity.Commands;

/// <summary>
/// Command to refresh an access token using a refresh token.
/// </summary>
public class RefreshTokenCommand : ICommand<AuthenticationResponse>
{
    /// <summary>
    /// Gets or sets the expired access token.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}
