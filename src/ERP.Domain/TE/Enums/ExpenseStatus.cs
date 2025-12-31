namespace ERP.Domain.TE.Enums;

/// <summary>
/// Expense report approval status.
/// </summary>
public enum ExpenseStatus : byte
{
    /// <summary>
    /// Expense report is in draft mode.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Expense report has been submitted for approval.
    /// </summary>
    Submitted = 2,

    /// <summary>
    /// Expense report has been approved.
    /// </summary>
    Approved = 3,

    /// <summary>
    /// Expense report has been rejected.
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Expense report has been recalled by the employee.
    /// </summary>
    Recalled = 5,

    /// <summary>
    /// Expense report has been reimbursed.
    /// </summary>
    Reimbursed = 6
}
