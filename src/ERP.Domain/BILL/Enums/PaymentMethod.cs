namespace ERP.Domain.BILL.Enums;

/// <summary>
/// Payment method
/// </summary>
public enum PaymentMethod : byte
{
    /// <summary>
    /// Cash payment
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Check payment
    /// </summary>
    Check = 2,

    /// <summary>
    /// Credit card payment
    /// </summary>
    CreditCard = 3,

    /// <summary>
    /// ACH/Bank transfer
    /// </summary>
    ACH = 4,

    /// <summary>
    /// Wire transfer
    /// </summary>
    Wire = 5,

    /// <summary>
    /// Other payment method
    /// </summary>
    Other = 99
}
