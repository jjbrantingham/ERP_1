using ERP.Application.Identity.Commands;
using ERP.Application.Identity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Login with username/email and password.
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>Authentication response with JWT tokens</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        _logger.LogInformation("Login attempt for user: {UserNameOrEmail}", command.UserNameOrEmail);

        // In a real implementation, use MediatR to dispatch the command
        // var response = await _mediator.Send(command);

        // For now, we'll create a handler instance directly (temporary until MediatR is added)
        var handler = HttpContext.RequestServices.GetRequiredService<LoginCommandHandler>();
        var response = await handler.Handle(command);

        _logger.LogInformation("User {UserName} logged in successfully", response.User.UserName);

        return Ok(response);
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <param name="command">Registration details</param>
    /// <returns>Authentication response with JWT tokens</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        _logger.LogInformation("Registration attempt for user: {UserName}", command.UserName);

        var handler = HttpContext.RequestServices.GetRequiredService<RegisterCommandHandler>();
        var response = await handler.Handle(command);

        _logger.LogInformation("User {UserName} registered successfully", response.User.UserName);

        return CreatedAtAction(nameof(GetCurrentUser), new { }, response);
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    /// <param name="command">Refresh token details</param>
    /// <returns>New authentication response with JWT tokens</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        _logger.LogInformation("Refresh token request");

        var handler = HttpContext.RequestServices.GetRequiredService<RefreshTokenCommandHandler>();
        var response = await handler.Handle(command);

        return Ok(response);
    }

    /// <summary>
    /// Change user password.
    /// </summary>
    /// <param name="command">Change password details</param>
    /// <returns>Success status</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        // Get user ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Password change request for user ID: {UserId}", userId);

        var changePasswordCommand = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = command.CurrentPassword,
            NewPassword = command.NewPassword
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ChangePasswordCommandHandler>();
        await handler.Handle(changePasswordCommand);

        _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);

        return NoContent();
    }

    /// <summary>
    /// Logout (invalidates refresh token).
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        // Get user ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogInformation("Logout request for user ID: {UserId}", userId);

            // Clear refresh token
            var userRepository = HttpContext.RequestServices.GetRequiredService<ERP.Application.Common.Interfaces.IUserRepository>();
            var unitOfWork = HttpContext.RequestServices.GetRequiredService<ERP.Application.Common.Interfaces.IUnitOfWork>();

            var user = await userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.ClearRefreshToken();
                await unitOfWork.SaveChangesAsync();
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Get current authenticated user information.
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var userRepository = HttpContext.RequestServices.GetRequiredService<ERP.Application.Common.Interfaces.IUserRepository>();
        var user = await userRepository.GetByIdWithRolesAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToList();

        var userDto = new UserDto
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
        };

        return Ok(userDto);
    }
}
