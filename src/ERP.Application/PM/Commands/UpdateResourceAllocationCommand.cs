using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to update an existing resource allocation.
/// </summary>
public class UpdateResourceAllocationCommand : IRequest<Unit>
{
    public long Id { get; init; }
    public decimal AllocatedHoursPerWeek { get; init; }
    public string? Role { get; init; }
    public string? Notes { get; init; }
    public DateTime? EndDate { get; init; }
}
