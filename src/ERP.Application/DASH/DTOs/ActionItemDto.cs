namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Action item for employee dashboard
/// </summary>
public class ActionItemDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public string Priority { get; set; } = "Normal";
    public string? LinkUrl { get; set; }
}
