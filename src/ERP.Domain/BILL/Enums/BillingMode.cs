namespace ERP.Domain.BILL.Enums;

/// <summary>
/// Billing mode determines how revenue is recognized
/// </summary>
public enum BillingMode : byte
{
    /// <summary>
    /// Time and Materials - bill based on actual hours and expenses
    /// </summary>
    TimeAndMaterials = 1,

    /// <summary>
    /// Fixed Price - bill fixed amount regardless of effort
    /// </summary>
    FixedPrice = 2,

    /// <summary>
    /// Percentage Complete - bill based on project completion percentage
    /// </summary>
    PercentageComplete = 3,

    /// <summary>
    /// Milestone - bill when specific milestones are achieved
    /// </summary>
    Milestone = 4,

    /// <summary>
    /// Retainer - recurring fixed amount billing
    /// </summary>
    Retainer = 5
}
