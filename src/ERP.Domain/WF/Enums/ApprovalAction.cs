namespace ERP.Domain.WF.Enums;

/// <summary>
/// Action taken on a workflow step
/// </summary>
public enum ApprovalAction : byte
{
    /// <summary>
    /// Step/item was approved
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Step/item was rejected
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Returned for revision
    /// </summary>
    ReturnedForRevision = 3,

    /// <summary>
    /// Escalated to higher authority
    /// </summary>
    Escalated = 4,

    /// <summary>
    /// Delegated to another approver
    /// </summary>
    Delegated = 5
}
