using ERP.Domain.RPT.Enums;

namespace ERP.Application.RPT.Services;

/// <summary>
/// Service for calculating report date ranges based on period type
/// </summary>
public interface IReportPeriodService
{
    /// <summary>
    /// Get date range for a report period
    /// </summary>
    (DateTime StartDate, DateTime EndDate) GetDateRange(ReportPeriod period, DateTime? customStart = null, DateTime? customEnd = null);

    /// <summary>
    /// Get start of current month
    /// </summary>
    DateTime GetStartOfMonth(DateTime date);

    /// <summary>
    /// Get end of current month
    /// </summary>
    DateTime GetEndOfMonth(DateTime date);

    /// <summary>
    /// Get start of current quarter
    /// </summary>
    DateTime GetStartOfQuarter(DateTime date);

    /// <summary>
    /// Get end of current quarter
    /// </summary>
    DateTime GetEndOfQuarter(DateTime date);

    /// <summary>
    /// Get start of current year
    /// </summary>
    DateTime GetStartOfYear(DateTime date);

    /// <summary>
    /// Get end of current year
    /// </summary>
    DateTime GetEndOfYear(DateTime date);
}
