using ERP.Domain.DASH.Enums;

namespace ERP.Application.DASH.DTOs;

/// <summary>
/// Represents a Key Performance Indicator metric
/// </summary>
public class KpiMetricDto
{
    /// <summary>
    /// Type of KPI
    /// </summary>
    public KpiType Type { get; set; }

    /// <summary>
    /// Display name of the KPI
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current value
    /// </summary>
    public decimal CurrentValue { get; set; }

    /// <summary>
    /// Previous period value for comparison
    /// </summary>
    public decimal? PreviousValue { get; set; }

    /// <summary>
    /// Target/goal value
    /// </summary>
    public decimal? TargetValue { get; set; }

    /// <summary>
    /// Percentage change from previous period
    /// </summary>
    public decimal? PercentageChange { get; set; }

    /// <summary>
    /// Trend direction
    /// </summary>
    public TrendDirection Trend { get; set; }

    /// <summary>
    /// Currency code (for monetary KPIs)
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Format hint (e.g., "currency", "percentage", "number", "hours")
    /// </summary>
    public string Format { get; set; } = "number";

    /// <summary>
    /// Is this KPI performing well (based on target/trend)?
    /// </summary>
    public bool IsHealthy { get; set; }
}
