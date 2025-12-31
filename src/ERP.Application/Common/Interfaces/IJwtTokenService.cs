using ERP.Domain.Identity.Entities;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token for a user.
    /// </summary>
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a token and returns the user ID if valid.
    /// </summary>
    long? ValidateToken(string token);

    /// <summary>
    /// Gets the expiry time for access tokens.
    /// </summary>
    DateTime GetTokenExpiry();
}
