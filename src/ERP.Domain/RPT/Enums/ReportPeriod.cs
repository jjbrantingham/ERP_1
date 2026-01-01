namespace ERP.Domain.RPT.Enums;

/// <summary>
/// Standard reporting periods
/// </summary>
public enum ReportPeriod : byte
{
    /// <summary>
    /// Current month to date
    /// </summary>
    MonthToDate = 1,

    /// <summary>
    /// Current quarter to date
    /// </summary>
    QuarterToDate = 2,

    /// <summary>
    /// Current year to date
    /// </summary>
    YearToDate = 3,

    /// <summary>
    /// Last month
    /// </summary>
    LastMonth = 4,

    /// <summary>
    /// Last quarter
    /// </summary>
    LastQuarter = 5,

    /// <summary>
    /// Last year
    /// </summary>
    LastYear = 6,

    /// <summary>
    /// Custom date range
    /// </summary>
    Custom = 99
}
