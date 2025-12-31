namespace ERP.Domain.TE.Enums;

/// <summary>
/// Expense category types.
/// </summary>
public enum ExpenseCategory : byte
{
    /// <summary>
    /// Travel expenses (flights, trains, etc.)
    /// </summary>
    Travel = 1,

    /// <summary>
    /// Lodging and accommodation.
    /// </summary>
    Lodging = 2,

    /// <summary>
    /// Meals and entertainment.
    /// </summary>
    Meals = 3,

    /// <summary>
    /// Transportation (taxi, rental car, mileage, etc.)
    /// </summary>
    Transportation = 4,

    /// <summary>
    /// Office supplies and materials.
    /// </summary>
    Supplies = 5,

    /// <summary>
    /// Equipment purchases.
    /// </summary>
    Equipment = 6,

    /// <summary>
    /// Training and education.
    /// </summary>
    Training = 7,

    /// <summary>
    /// Communication (phone, internet, etc.)
    /// </summary>
    Communication = 8,

    /// <summary>
    /// Client entertainment.
    /// </summary>
    Entertainment = 9,

    /// <summary>
    /// Other miscellaneous expenses.
    /// </summary>
    Other = 99
}
