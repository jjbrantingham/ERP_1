namespace ERP.Domain.Identity.Entities;

/// <summary>
/// Join entity representing the many-to-many relationship between users and roles.
/// </summary>
public class UserRole
{
    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets the user navigation property.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets the role ID.
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// Gets the role navigation property.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Gets the date when the role was assigned.
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
}
