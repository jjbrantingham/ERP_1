namespace ERP.Domain.VM.Enums;

/// <summary>
/// Vendor status in the system.
/// </summary>
public enum VendorStatus : byte
{
    /// <summary>
    /// Vendor is being evaluated or onboarded.
    /// </summary>
    Prospective = 1,

    /// <summary>
    /// Vendor is active and can be used.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Vendor is temporarily on hold.
    /// </summary>
    OnHold = 3,

    /// <summary>
    /// Vendor is inactive and should not be used.
    /// </summary>
    Inactive = 4,

    /// <summary>
    /// Vendor is blacklisted and cannot be used.
    /// </summary>
    Blacklisted = 5
}
