using ERP.Domain.Common;

namespace ERP.Domain.Identity.Entities;

/// <summary>
/// Represents a role in the system.
/// Roles define sets of permissions that can be assigned to users.
/// </summary>
public class Role : AggregateRoot
{
    private readonly List<RolePermission> _rolePermissions = new();

    /// <summary>
    /// Gets the role name (unique within tenant).
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the normalized role name for lookups.
    /// </summary>
    public string NormalizedName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the role description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this is a system role (cannot be deleted).
    /// </summary>
    public bool IsSystemRole { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this role is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the role's permissions.
    /// </summary>
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { } // EF Core

    /// <summary>
    /// Creates a new role.
    /// </summary>
    public static Role Create(
        Guid tenantId,
        string name,
        string? description = null,
        bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        var role = new Role
        {
            TenantId = tenantId,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            IsSystemRole = isSystemRole,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        return role;
    }

    /// <summary>
    /// Updates the role information.
    /// </summary>
    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a permission to the role.
    /// </summary>
    public void AddPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission is required.", nameof(permission));

        if (_rolePermissions.Any(rp => rp.Permission == permission))
            return;

        _rolePermissions.Add(new RolePermission
        {
            RoleId = Id,
            Permission = permission
        });
    }

    /// <summary>
    /// Removes a permission from the role.
    /// </summary>
    public void RemovePermission(string permission)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.Permission == permission);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
        }
    }

    /// <summary>
    /// Checks if the role has a specific permission.
    /// </summary>
    public bool HasPermission(string permission)
    {
        return _rolePermissions.Any(rp => rp.Permission == permission);
    }

    /// <summary>
    /// Activates the role.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the role.
    /// </summary>
    public void Deactivate()
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot deactivate a system role.");

        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }
}
