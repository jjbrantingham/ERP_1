using ERP.Domain.Common;

namespace ERP.Domain.HR.Entities;

/// <summary>
/// Represents a type of resource (e.g., Developer, Designer, Project Manager).
/// Resource types are used to categorize employees and define default rates.
/// </summary>
public class ResourceType : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? Code { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    // Navigation properties for rates
    private readonly List<Rate> _rates = new();
    public IReadOnlyCollection<Rate> Rates => _rates.AsReadOnly();

    private ResourceType()
    {
        Name = string.Empty;
    }

    /// <summary>
    /// Creates a new resource type.
    /// </summary>
    public static ResourceType Create(
        Guid tenantId,
        string name,
        string? description = null,
        string? code = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource type name is required", nameof(name));

        var resourceType = new ResourceType
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            Code = code?.Trim()?.ToUpperInvariant(),
            IsActive = true,
            DisplayOrder = displayOrder,
            CreatedDate = DateTime.UtcNow
        };

        return resourceType;
    }

    /// <summary>
    /// Updates the resource type information.
    /// </summary>
    public void Update(string name, string? description = null, string? code = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource type name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Code = code?.Trim()?.ToUpperInvariant();
        DisplayOrder = displayOrder;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the resource type.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the resource type.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }
}
