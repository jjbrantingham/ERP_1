using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;

namespace ERP.Application.Identity.Commands;

/// <summary>
/// Handler for changing user password.
/// </summary>
public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Validate new passwords match
        if (command.NewPassword != command.ConfirmNewPassword)
        {
            throw new ValidationException("NewPassword", "Passwords do not match.");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is incorrect.");
        }

        // Hash new password
        var newPasswordHash = _passwordHasher.HashPassword(command.NewPassword);

        // Change password
        user.ChangePassword(newPasswordHash);

        // Clear refresh tokens (force re-login on all devices)
        user.ClearRefreshToken();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
