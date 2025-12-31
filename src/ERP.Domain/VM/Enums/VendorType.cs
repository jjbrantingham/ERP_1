namespace ERP.Domain.VM.Enums;

/// <summary>
/// Vendor classification types.
/// </summary>
public enum VendorType : byte
{
    /// <summary>
    /// Individual contractor providing services.
    /// </summary>
    Contractor = 1,

    /// <summary>
    /// Supplier of goods or materials.
    /// </summary>
    Supplier = 2,

    /// <summary>
    /// Professional services firm.
    /// </summary>
    ProfessionalServices = 3,

    /// <summary>
    /// Software or technology vendor.
    /// </summary>
    Software = 4,

    /// <summary>
    /// Consultant providing specialized expertise.
    /// </summary>
    Consultant = 5,

    /// <summary>
    /// Other vendor type.
    /// </summary>
    Other = 99
}
