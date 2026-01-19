using System.Reflection;
using System.Text;
using System.Text.Json;
using ERP.Application.RPT.Services;
using ERP.Domain.RPT.Enums;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Service for exporting reports to various formats (PDF, Excel, CSV)
/// NOTE: This is a basic implementation. For production use, consider adding:
/// - PDF: iTextSharp, QuestPDF, or PuppeteerSharp
/// - Excel: EPPlus, ClosedXML, or NPOI
/// </summary>
public class ReportExportService : IReportExportService
{
    public async Task<byte[]> ExportReportAsync<T>(T data, ExportFormat format, string reportTitle, CancellationToken cancellationToken = default)
    {
        return format switch
        {
            ExportFormat.CSV => await ExportToCsvAsync<object>(data is IEnumerable<object> enumerable ? enumerable : new object[] { data }, true, cancellationToken),
            ExportFormat.Excel => await ExportToExcelAsync<object>(data is IEnumerable<object> enumerable ? enumerable : new object[] { data }, reportTitle, true, cancellationToken),
            ExportFormat.PDF => await ExportToPdfAsync(ConvertToHtml(data, reportTitle), reportTitle, cancellationToken),
            _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
        };
    }

    public Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, bool includeHeaders = true, CancellationToken cancellationToken = default)
    {
        var csv = new StringBuilder();
        var items = data.ToList();

        if (!items.Any())
            return Task.FromResult(Encoding.UTF8.GetBytes(string.Empty));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Add headers
        if (includeHeaders)
        {
            csv.AppendLine(string.Join(",", properties.Select(p => EscapeCsvValue(p.Name))));
        }

        // Add data rows
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsvValue(value?.ToString() ?? string.Empty);
            });
            csv.AppendLine(string.Join(",", values));
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    public Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName, bool includeHeaders = true, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Excel export using EPPlus or ClosedXML
        // For now, return CSV as a fallback
        // Example with EPPlus:
        // using (var package = new ExcelPackage())
        // {
        //     var worksheet = package.Workbook.Worksheets.Add(sheetName);
        //     worksheet.Cells["A1"].LoadFromCollection(data, includeHeaders);
        //     return Task.FromResult(package.GetAsByteArray());
        // }

        // Fallback to CSV for basic implementation
        return ExportToCsvAsync(data, includeHeaders, cancellationToken);
    }

    public Task<byte[]> ExportToPdfAsync(string htmlContent, string reportTitle, CancellationToken cancellationToken = default)
    {
        // TODO: Implement PDF export using iTextSharp, QuestPDF, or PuppeteerSharp
        // Example with QuestPDF:
        // var document = Document.Create(container =>
        // {
        //     container.Page(page =>
        //     {
        //         page.Header().Text(reportTitle).FontSize(20);
        //         page.Content().Text(htmlContent);
        //     });
        // });
        // return Task.FromResult(document.GeneratePdf());

        // Example with PuppeteerSharp (HTML to PDF):
        // await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        // await using var page = await browser.NewPageAsync();
        // await page.SetContentAsync(htmlContent);
        // return await page.PdfDataAsync();

        // For basic implementation, return HTML as bytes with a message
        var fallbackContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>{reportTitle}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #333; }}
        .note {{ background-color: #fff3cd; padding: 10px; border-left: 4px solid #ffc107; margin: 20px 0; }}
    </style>
</head>
<body>
    <h1>{reportTitle}</h1>
    <div class='note'>
        <strong>Note:</strong> PDF export requires a PDF library to be installed (e.g., iTextSharp, QuestPDF, or PuppeteerSharp).
        This is currently returning HTML content as a fallback.
    </div>
    <div>
        {htmlContent}
    </div>
</body>
</html>";

        return Task.FromResult(Encoding.UTF8.GetBytes(fallbackContent));
    }

    public string GetMimeType(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.PDF => "application/pdf",
            ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ExportFormat.CSV => "text/csv",
            _ => "application/octet-stream"
        };
    }

    public string GetFileExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.PDF => ".pdf",
            ExportFormat.Excel => ".xlsx",
            ExportFormat.CSV => ".csv",
            _ => ".dat"
        };
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape quotes and wrap in quotes if needed
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private string ConvertToHtml<T>(T data, string reportTitle)
    {
        var html = new StringBuilder();
        html.AppendLine($"<h2>{reportTitle}</h2>");

        if (data is IEnumerable<object> enumerable)
        {
            var items = enumerable.ToList();
            if (items.Any())
            {
                var properties = items[0].GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse;'>");
                html.AppendLine("<thead><tr>");
                foreach (var prop in properties)
                {
                    html.AppendLine($"<th>{prop.Name}</th>");
                }
                html.AppendLine("</tr></thead>");

                html.AppendLine("<tbody>");
                foreach (var item in items)
                {
                    html.AppendLine("<tr>");
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(item);
                        html.AppendLine($"<td>{value}</td>");
                    }
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</tbody>");
                html.AppendLine("</table>");
            }
        }
        else
        {
            // Single object - display as key-value pairs
            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse;'>");
            foreach (var prop in properties)
            {
                var value = prop.GetValue(data);
                html.AppendLine($"<tr><th>{prop.Name}</th><td>{value}</td></tr>");
            }
            html.AppendLine("</table>");
        }

        return html.ToString();
    }
}
