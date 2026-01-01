using ERP.Application.RPT.DTOs;
using MediatR;

namespace ERP.Application.RPT.Queries;

public class GetBalanceSheetQuery : IRequest<BalanceSheetDto>
{
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow;
    public string Currency { get; set; } = "USD";
}
