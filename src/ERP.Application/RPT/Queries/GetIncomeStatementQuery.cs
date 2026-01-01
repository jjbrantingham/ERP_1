using ERP.Application.RPT.DTOs;
using MediatR;

namespace ERP.Application.RPT.Queries;

public class GetIncomeStatementQuery : IRequest<IncomeStatementDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Currency { get; set; } = "USD";
}
