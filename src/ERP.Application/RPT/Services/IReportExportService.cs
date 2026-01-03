using ERP.Domain.RPT.Enums;

namespace ERP.Application.RPT.Services;

/// <summary>
/// Service for exporting reports to various formats (PDF, Excel, CSV)
/// </summary>
public interface IReportExportService
{
    /// <summary>
    /// Export report data to the specified format
    /// </summary>
    /// <typeparam name="T">The type of report data</typeparam>
    /// <param name="data">The report data to export</param>
    /// <param name="format">The export format (PDF, Excel, CSV)</param>
    /// <param name="reportTitle">The title of the report</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Byte array containing the exported report</returns>
    Task<byte[]> ExportReportAsync<T>(T data, ExportFormat format, string reportTitle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export tabular data to CSV format
    /// </summary>
    /// <typeparam name="T">The type of data rows</typeparam>
    /// <param name="data">Collection of data rows</param>
    /// <param name="includeHeaders">Whether to include column headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Byte array containing the CSV file</returns>
    Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, bool includeHeaders = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export tabular data to Excel format
    /// </summary>
    /// <typeparam name="T">The type of data rows</typeparam>
    /// <param name="data">Collection of data rows</param>
    /// <param name="sheetName">Name of the Excel worksheet</param>
    /// <param name="includeHeaders">Whether to include column headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Byte array containing the Excel file</returns>
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName, bool includeHeaders = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export report to PDF format
    /// </summary>
    /// <param name="htmlContent">HTML content to convert to PDF</param>
    /// <param name="reportTitle">The title of the report</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Byte array containing the PDF file</returns>
    Task<byte[]> ExportToPdfAsync(string htmlContent, string reportTitle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the MIME type for the export format
    /// </summary>
    /// <param name="format">The export format</param>
    /// <returns>MIME type string</returns>
    string GetMimeType(ExportFormat format);

    /// <summary>
    /// Get the file extension for the export format
    /// </summary>
    /// <param name="format">The export format</param>
    /// <returns>File extension (e.g., ".pdf", ".xlsx", ".csv")</returns>
    string GetFileExtension(ExportFormat format);
}
