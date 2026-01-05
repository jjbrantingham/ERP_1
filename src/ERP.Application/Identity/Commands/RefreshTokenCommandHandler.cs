using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.Identity.DTOs;

namespace ERP.Application.Identity.Commands;

/// <summary>
/// Handler for refreshing access tokens.
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AuthenticationResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        // Validate the access token (even if expired) to get user ID
        var userId = _jwtTokenService.ValidateToken(command.AccessToken);

        if (userId == null)
        {
            throw new UnauthorizedException("Invalid access token.");
        }

        // Get user with roles
        var user = await _userRepository.GetByIdWithRolesAsync(userId.Value, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        // Validate refresh token
        if (user.RefreshToken != command.RefreshToken)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token has expired. Please login again.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is inactive.");
        }

        // Get roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToList();

        // Generate new tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var tokenExpiry = _jwtTokenService.GetTokenExpiry();

        // Update refresh token
        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Build response
        return new AuthenticationResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = tokenExpiry,
            User = new UserDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                UserName = user.UserName,
                Email = user.Email.Value,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                LastLoginDate = user.LastLoginDate,
                Roles = roles,
                Permissions = permissions
            }
        };
    }
}
