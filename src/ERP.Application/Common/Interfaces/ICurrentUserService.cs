namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Service to get the current authenticated user's information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID (from claims)
    /// </summary>
    long? UserId { get; }

    /// <summary>
    /// Gets the current user's username/email
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Indicates whether a user is currently authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}
