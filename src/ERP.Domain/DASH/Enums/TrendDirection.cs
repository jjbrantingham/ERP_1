namespace ERP.Domain.DASH.Enums;

/// <summary>
/// Trend direction for KPI metrics
/// </summary>
public enum TrendDirection : byte
{
    /// <summary>
    /// Metric is trending upward
    /// </summary>
    Up = 1,

    /// <summary>
    /// Metric is trending downward
    /// </summary>
    Down = 2,

    /// <summary>
    /// Metric is stable/flat
    /// </summary>
    Flat = 3,

    /// <summary>
    /// Not enough data to determine trend
    /// </summary>
    Unknown = 4
}
