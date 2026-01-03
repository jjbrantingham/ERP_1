using ERP.Application.RPT.Services;
using ERP.Domain.RPT.Enums;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Service for calculating report date ranges based on period type
/// </summary>
public class ReportPeriodService : IReportPeriodService
{
    public (DateTime StartDate, DateTime EndDate) GetDateRange(ReportPeriod period, DateTime? customStart = null, DateTime? customEnd = null)
    {
        var today = DateTime.UtcNow.Date;

        return period switch
        {
            ReportPeriod.Custom => (customStart ?? today, customEnd ?? today),
            ReportPeriod.Today => (today, today),
            ReportPeriod.ThisWeek => GetThisWeek(today),
            ReportPeriod.ThisMonth => (GetStartOfMonth(today), GetEndOfMonth(today)),
            ReportPeriod.ThisQuarter => (GetStartOfQuarter(today), GetEndOfQuarter(today)),
            ReportPeriod.ThisYear => (GetStartOfYear(today), GetEndOfYear(today)),
            ReportPeriod.LastMonth => GetLastMonth(today),
            ReportPeriod.LastQuarter => GetLastQuarter(today),
            ReportPeriod.LastYear => GetLastYear(today),
            ReportPeriod.YearToDate => (GetStartOfYear(today), today),
            ReportPeriod.QuarterToDate => (GetStartOfQuarter(today), today),
            _ => (today, today)
        };
    }

    public DateTime GetStartOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public DateTime GetEndOfMonth(DateTime date)
    {
        return GetStartOfMonth(date).AddMonths(1).AddDays(-1);
    }

    public DateTime GetStartOfQuarter(DateTime date)
    {
        var quarter = (date.Month - 1) / 3;
        return new DateTime(date.Year, quarter * 3 + 1, 1);
    }

    public DateTime GetEndOfQuarter(DateTime date)
    {
        return GetStartOfQuarter(date).AddMonths(3).AddDays(-1);
    }

    public DateTime GetStartOfYear(DateTime date)
    {
        return new DateTime(date.Year, 1, 1);
    }

    public DateTime GetEndOfYear(DateTime date)
    {
        return new DateTime(date.Year, 12, 31);
    }

    private (DateTime, DateTime) GetThisWeek(DateTime date)
    {
        var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);
        return (startOfWeek, endOfWeek);
    }

    private (DateTime, DateTime) GetLastMonth(DateTime date)
    {
        var lastMonth = date.AddMonths(-1);
        return (GetStartOfMonth(lastMonth), GetEndOfMonth(lastMonth));
    }

    private (DateTime, DateTime) GetLastQuarter(DateTime date)
    {
        var lastQuarter = date.AddMonths(-3);
        return (GetStartOfQuarter(lastQuarter), GetEndOfQuarter(lastQuarter));
    }

    private (DateTime, DateTime) GetLastYear(DateTime date)
    {
        var lastYear = date.AddYears(-1);
        return (GetStartOfYear(lastYear), GetEndOfYear(lastYear));
    }
}
