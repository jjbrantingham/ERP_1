using ERP.Application.Common.Interfaces;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Command to create a new resource type.
/// </summary>
public class CreateResourceTypeCommand : ICommand<long>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Code { get; init; }
    public int DisplayOrder { get; init; } = 0;
}
