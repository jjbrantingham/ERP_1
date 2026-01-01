namespace ERP.Domain.BILL.Enums;

/// <summary>
/// Status of an invoice
/// </summary>
public enum InvoiceStatus : byte
{
    /// <summary>
    /// Draft - can be edited
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Sent - sent to customer, awaiting payment
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Posted - posted to accounting system
    /// </summary>
    Posted = 3,

    /// <summary>
    /// Partially Paid - some payment received
    /// </summary>
    PartiallyPaid = 4,

    /// <summary>
    /// Paid - fully paid
    /// </summary>
    Paid = 5,

    /// <summary>
    /// Overdue - past due date
    /// </summary>
    Overdue = 6,

    /// <summary>
    /// Cancelled - cancelled before sending
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// Voided - voided after posting
    /// </summary>
    Voided = 8
}
