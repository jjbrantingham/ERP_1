namespace ERP.Domain.WF.Enums;

/// <summary>
/// Status of a workflow instance (runtime execution)
/// </summary>
public enum WorkflowInstanceStatus : byte
{
    /// <summary>
    /// Workflow has been initiated but not started
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Workflow is currently in progress
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Workflow completed successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Workflow was rejected at some step
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Workflow was cancelled
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Workflow encountered an error
    /// </summary>
    Error = 6
}
