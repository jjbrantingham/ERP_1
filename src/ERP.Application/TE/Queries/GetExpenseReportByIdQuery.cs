using ERP.Application.Common.Interfaces;
namespace ERP.Application.TE.DTOs;

namespace ERP.Application.TE.Queries;

public class GetExpenseReportByIdQuery : IQuery<ExpenseReportDto>
{
    public long ExpenseReportId { get; set; }
}
