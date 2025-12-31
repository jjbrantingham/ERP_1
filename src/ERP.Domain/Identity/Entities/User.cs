using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.Identity.Events;

namespace ERP.Domain.Identity.Entities;

/// <summary>
/// Represents a user in the system.
/// Users belong to a tenant and can have multiple roles.
/// </summary>
public class User : AggregateRoot
{
    private readonly List<UserRole> _userRoles = new();

    /// <summary>
    /// Gets the username (unique within tenant).
    /// </summary>
    public string UserName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Gets the password hash.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the email is confirmed.
    /// </summary>
    public bool EmailConfirmed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether two-factor authentication is enabled.
    /// </summary>
    public bool TwoFactorEnabled { get; private set; }

    /// <summary>
    /// Gets the number of failed login attempts.
    /// </summary>
    public int AccessFailedCount { get; private set; }

    /// <summary>
    /// Gets the lockout end date (null if not locked out).
    /// </summary>
    public DateTime? LockoutEnd { get; private set; }

    /// <summary>
    /// Gets a value indicating whether lockout is enabled for this user.
    /// </summary>
    public bool LockoutEnabled { get; private set; }

    /// <summary>
    /// Gets the last login date.
    /// </summary>
    public DateTime? LastLoginDate { get; private set; }

    /// <summary>
    /// Gets the refresh token for JWT authentication.
    /// </summary>
    public string? RefreshToken { get; private set; }

    /// <summary>
    /// Gets the refresh token expiry date.
    /// </summary>
    public DateTime? RefreshTokenExpiry { get; private set; }

    /// <summary>
    /// Gets the user's roles.
    /// </summary>
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() { } // EF Core

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public static User Create(
        Guid tenantId,
        string userName,
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username is required.", nameof(userName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        var user = new User
        {
            TenantId = tenantId,
            UserName = userName,
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            IsActive = true,
            EmailConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            CreatedDate = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email.Value, user.FullName));

        return user;
    }

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new UserPasswordChangedEvent(Id, Email.Value));
    }

    /// <summary>
    /// Confirms the user's email address.
    /// </summary>
    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the user.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("User is already active.");

        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is already inactive.");

        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a successful login.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginDate = DateTime.UtcNow;
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    public void RecordFailedLogin(int maxFailedAttempts = 5, int lockoutMinutes = 15)
    {
        AccessFailedCount++;

        if (LockoutEnabled && AccessFailedCount >= maxFailedAttempts)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            AddDomainEvent(new UserLockedOutEvent(Id, Email.Value, LockoutEnd.Value));
        }
    }

    /// <summary>
    /// Checks if the user is currently locked out.
    /// </summary>
    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Unlocks the user account.
    /// </summary>
    public void Unlock()
    {
        LockoutEnd = null;
        AccessFailedCount = 0;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the refresh token for JWT authentication.
    /// </summary>
    public void SetRefreshToken(string refreshToken, DateTime expiry)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiry = expiry;
    }

    /// <summary>
    /// Clears the refresh token.
    /// </summary>
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
    }

    /// <summary>
    /// Adds a role to the user.
    /// </summary>
    public void AddRole(long roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            return;

        _userRoles.Add(new UserRole { UserId = Id, RoleId = roleId });
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    public void RemoveRole(long roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
        }
    }
}
