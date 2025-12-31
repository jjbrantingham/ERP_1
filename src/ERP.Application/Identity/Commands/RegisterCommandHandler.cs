using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Identity.DTOs;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.Identity.Entities;
using ERP.Shared.Constants;

namespace ERP.Application.Identity.Commands;

/// <summary>
/// Handler for user registration command.
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ICurrentTenantService currentTenantService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _currentTenantService = currentTenantService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthenticationResponse> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        // Validate passwords match
        if (command.Password != command.ConfirmPassword)
        {
            throw new ValidationException("Password", "Passwords do not match.");
        }

        // Get tenant ID
        var tenantId = command.TenantId ?? _currentTenantService.TenantId;

        // Check if username already exists
        if (await _userRepository.UserNameExistsAsync(command.UserName, cancellationToken))
        {
            throw new ValidationException("UserName", "Username is already taken.");
        }

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(command.Email, cancellationToken))
        {
            throw new ValidationException("Email", "Email is already registered.");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        // Create user
        var user = User.Create(
            tenantId,
            command.UserName,
            new Email(command.Email),
            passwordHash,
            command.FirstName,
            command.LastName,
            command.PhoneNumber
        );

        // Assign default Employee role
        var employeeRole = await _roleRepository.GetByNameAsync(Roles.Employee, cancellationToken);
        if (employeeRole != null)
        {
            user.AddRole(employeeRole.Id);
        }

        // Save user
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload user with roles
        user = await _userRepository.GetByIdWithRolesAsync(user.Id, cancellationToken);

        if (user == null)
        {
            throw new Exception("Failed to create user.");
        }

        // Get roles and permissions
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

        // Set refresh token
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
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
