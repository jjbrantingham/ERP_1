namespace ERP.Domain.WF.Enums;

/// <summary>
/// Status of a workflow definition
/// </summary>
public enum WorkflowDefinitionStatus : byte
{
    /// <summary>
    /// Workflow is being designed/configured
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Workflow is active and can be used
    /// </summary>
    Active = 2,

    /// <summary>
    /// Workflow is inactive (archived/deprecated)
    /// </summary>
    Inactive = 3
}
