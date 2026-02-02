using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to delete a resource allocation.
/// </summary>
public class DeleteResourceAllocationCommand : IRequest<Unit>
{
    public long Id { get; init; }
}
