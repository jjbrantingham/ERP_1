namespace ERP.Domain.PM.Enums;

/// <summary>
/// Types of contracts.
/// </summary>
public enum ContractType : byte
{
    /// <summary>
    /// Time and materials contract.
    /// </summary>
    TimeAndMaterials = 1,

    /// <summary>
    /// Fixed price contract.
    /// </summary>
    FixedPrice = 2,

    /// <summary>
    /// Cost plus contract.
    /// </summary>
    CostPlus = 3,

    /// <summary>
    /// Not to exceed (NTE) contract.
    /// </summary>
    NotToExceed = 4,

    /// <summary>
    /// Retainer contract.
    /// </summary>
    Retainer = 5
}
