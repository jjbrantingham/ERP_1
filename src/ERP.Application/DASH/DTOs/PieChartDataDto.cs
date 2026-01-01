namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Data point for pie/donut charts
/// </summary>
public class PieChartDataDto
{
    /// <summary>
    /// Category/segment label
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Value
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Percentage of total
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Optional color hint
    /// </summary>
    public string? Color { get; set; }
}
