using ERP.Application.DASH.DTOs;
using MediatR;

namespace ERP.Application.DASH.Queries;

/// <summary>
/// Query to get project manager dashboard data
/// </summary>
public class GetProjectDashboardQuery : IRequest<ProjectDashboardDto>
{
    /// <summary>
    /// As of date (defaults to today)
    /// </summary>
    public DateTime? AsOfDate { get; set; }

    /// <summary>
    /// Currency for financial metrics (defaults to USD)
    /// </summary>
    public string Currency { get; set; } = "USD";
}
