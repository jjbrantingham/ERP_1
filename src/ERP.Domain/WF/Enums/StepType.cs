namespace ERP.Domain.WF.Enums;

/// <summary>
/// Type of workflow step
/// </summary>
public enum StepType : byte
{
    /// <summary>
    /// Requires human approval/rejection
    /// </summary>
    Approval = 1,

    /// <summary>
    /// Automatic step (rule-based)
    /// </summary>
    Automated = 2,

    /// <summary>
    /// Notification step (inform users)
    /// </summary>
    Notification = 3,

    /// <summary>
    /// Conditional branching
    /// </summary>
    Conditional = 4
}
