namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Data point for line/bar charts
/// </summary>
public class ChartDataPointDto
{
    /// <summary>
    /// Label (date, category name, etc.)
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Value
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Optional secondary value (for comparison charts)
    /// </summary>
    public decimal? SecondaryValue { get; set; }

    /// <summary>
    /// Date/time for time-series data
    /// </summary>
    public DateTime? Date { get; set; }
}
