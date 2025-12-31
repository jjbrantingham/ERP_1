namespace ERP.Domain.Identity.Entities;

/// <summary>
/// Represents a permission assigned to a role.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets the role ID.
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// Gets the role navigation property.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Gets the permission name (e.g., "Projects.Create", "Invoices.Approve").
    /// </summary>
    public string Permission { get; set; } = string.Empty;

    /// <summary>
    /// Gets the date when the permission was granted.
    /// </summary>
    public DateTime GrantedDate { get; set; } = DateTime.UtcNow;
}
