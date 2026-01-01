namespace ERP.Domain.RPT.Enums;

/// <summary>
/// Export formats for reports
/// </summary>
public enum ReportFormat : byte
{
    /// <summary>
    /// JSON format for API consumption
    /// </summary>
    Json = 1,

    /// <summary>
    /// PDF format for printing
    /// </summary>
    Pdf = 2,

    /// <summary>
    /// Excel format for analysis
    /// </summary>
    Excel = 3,

    /// <summary>
    /// CSV format for data export
    /// </summary>
    Csv = 4,

    /// <summary>
    /// HTML format for web display
    /// </summary>
    Html = 5
}
