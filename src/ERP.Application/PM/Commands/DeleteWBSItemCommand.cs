using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to delete a WBS item.
/// </summary>
public class DeleteWBSItemCommand : IRequest<Unit>
{
    public long Id { get; init; }
}
