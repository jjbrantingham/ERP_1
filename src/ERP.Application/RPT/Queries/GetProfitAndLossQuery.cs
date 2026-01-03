using ERP.Application.RPT.DTOs;
using ERP.Domain.RPT.Enums;
using MediatR;

namespace ERP.Application.RPT.Queries;

/// <summary>
/// Query to get Profit and Loss (Income Statement) report
/// </summary>
public class GetProfitAndLossQuery : IRequest<ProfitAndLossDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReportPeriod Period { get; set; } = ReportPeriod.ThisMonth;
}
