using ERP.Application.AUDIT.DTOs;
using ERP.Application.Common.Models;
using MediatR;

namespace ERP.Application.AUDIT.Queries;

/// <summary>
/// Query to get activity logs for a specific user.
/// </summary>
public class GetUserActivityQuery : IRequest<PagedResult<AuditLogDto>>
{
    public long UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
