namespace ERP.Domain.BILL.Enums;

/// <summary>
/// Status of a payment
/// </summary>
public enum PaymentStatus : byte
{
    /// <summary>
    /// Pending - payment initiated but not cleared
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Cleared - payment cleared and applied
    /// </summary>
    Cleared = 2,

    /// <summary>
    /// Failed - payment failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Reversed - payment reversed/refunded
    /// </summary>
    Reversed = 4
}
