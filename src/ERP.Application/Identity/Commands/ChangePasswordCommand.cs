using ERP.Application.Common.Interfaces;

namespace ERP.Application.Identity.Commands;

/// <summary>
/// Command to change a user's password.
/// </summary>
public class ChangePasswordCommand : ICommand
{
    /// <summary>
    /// Gets or sets the user ID (resolved from current user context).
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets or sets the current password.
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password confirmation.
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
