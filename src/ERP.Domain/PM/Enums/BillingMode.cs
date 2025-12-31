namespace ERP.Domain.PM.Enums;

/// <summary>
/// Billing modes for projects.
/// </summary>
public enum BillingMode : byte
{
    /// <summary>
    /// Time and materials billing.
    /// </summary>
    TimeAndMaterials = 1,

    /// <summary>
    /// Fixed price billing.
    /// </summary>
    FixedPrice = 2,

    /// <summary>
    /// Percentage complete billing.
    /// </summary>
    PercentComplete = 3,

    /// <summary>
    /// Milestone-based billing.
    /// </summary>
    Milestone = 4,

    /// <summary>
    /// Non-billable (overhead/internal).
    /// </summary>
    NonBillable = 5
}
