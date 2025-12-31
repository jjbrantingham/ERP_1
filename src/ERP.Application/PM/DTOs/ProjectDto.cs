namespace ERP.Application.PM.DTOs;

/// <summary>
/// Data transfer object for Project.
/// </summary>
public class ProjectDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public long ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProjectType { get; set; } = string.Empty;
    public string BillingMode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string? BudgetCurrency { get; set; }
    public long? ProjectManagerId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
