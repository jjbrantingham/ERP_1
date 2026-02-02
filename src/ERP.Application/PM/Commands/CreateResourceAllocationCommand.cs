using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to create a new resource allocation.
/// </summary>
public class CreateResourceAllocationCommand : IRequest<long>
{
    public long ProjectId { get; init; }
    public long EmployeeId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal AllocatedHoursPerWeek { get; init; }
    public string? Role { get; init; }
    public string? Notes { get; init; }
}
