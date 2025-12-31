using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Identity.DTOs;

namespace ERP.Application.Identity.Commands;

/// <summary>
/// Handler for login command.
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthenticationResponse> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        // Find user by username or email
        var user = await _userRepository.GetByUserNameWithRolesAsync(command.UserNameOrEmail, cancellationToken);

        if (user == null)
        {
            // Try email
            user = await _userRepository.GetByEmailAsync(command.UserNameOrEmail, cancellationToken);
        }

        if (user == null)
        {
            throw new UnauthorizedException("Invalid username/email or password.");
        }

        // Check if user is locked out
        if (user.IsLockedOut())
        {
            throw new UnauthorizedException($"Account is locked until {user.LockoutEnd:yyyy-MM-dd HH:mm:ss} UTC.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is inactive. Please contact administrator.");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            // Record failed login
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("Invalid username/email or password.");
        }

        // Get user roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToList();

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var tokenExpiry = _jwtTokenService.GetTokenExpiry();

        // Set refresh token with appropriate expiry
        var refreshTokenExpiry = command.RememberMe
            ? DateTime.UtcNow.AddDays(30)
            : DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken(refreshToken, refreshTokenExpiry);

        // Record successful login
        user.RecordSuccessfulLogin();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Build response
        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
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
