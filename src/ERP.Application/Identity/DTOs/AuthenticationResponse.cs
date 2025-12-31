namespace ERP.Application.Identity.DTOs;

/// <summary>
/// DTO for authentication response containing JWT tokens.
/// </summary>
public class AuthenticationResponse
{
    /// <summary>
    /// Gets or sets the access token (JWT).
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token expiry date/time (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the user information.
    /// </summary>
    public UserDto User { get; set; } = null!;
}
