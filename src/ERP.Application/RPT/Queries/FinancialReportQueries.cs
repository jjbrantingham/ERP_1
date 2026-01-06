using ERP.Application.RPT.DTOs;
using ERP.Domain.RPT.Enums;
using MediatR;

namespace ERP.Application.RPT.Queries;

/// <summary>
/// Query to get Cash Flow Statement report
/// </summary>
public class GetCashFlowQuery : IRequest<CashFlowDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReportPeriod Period { get; set; } = ReportPeriod.ThisMonth;
}

/// <summary>
/// Query to get AR Aging report
/// </summary>
public class GetARAgingQuery : IRequest<ARAgingDto>
{
    public DateTime? AsOfDate { get; set; }
}
