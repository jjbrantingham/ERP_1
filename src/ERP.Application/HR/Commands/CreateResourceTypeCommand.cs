using ERP.Application.Common.Interfaces;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Command to create a new resource type.
/// </summary>
public class CreateResourceTypeCommand : ICommand<long>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public int DisplayOrder { get; set; } = 0;
}
