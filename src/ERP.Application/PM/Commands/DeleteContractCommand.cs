using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to delete a contract.
/// </summary>
public class DeleteContractCommand : IRequest<Unit>
{
    public long Id { get; init; }
}
