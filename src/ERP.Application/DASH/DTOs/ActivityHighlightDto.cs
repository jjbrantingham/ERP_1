namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Recent activity highlight for dashboard
/// </summary>
public class ActivityHighlightDto
{
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string? UserName { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}
