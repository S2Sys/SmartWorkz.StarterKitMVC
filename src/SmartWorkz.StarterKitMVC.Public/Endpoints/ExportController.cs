using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core.External.Export;
using System.Net.Mime;

namespace SmartWorkz.StarterKitMVC.Public.Endpoints;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IPdfExporter _pdfExporter;
    private readonly IExcelExporter _excelExporter;
    private readonly ILogger<ExportController> _logger;

    public ExportController(
        IPdfExporter pdfExporter,
        IExcelExporter excelExporter,
        ILogger<ExportController> logger)
    {
        _pdfExporter = pdfExporter;
        _excelExporter = excelExporter;
        _logger = logger;
    }

    /// <summary>
    /// Export data to PDF or Excel format
    /// </summary>
    /// <param name="request">Export request with data, title, and format</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>File download (PDF or Excel)</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Export(
        [FromBody] ExportRequest request,
        CancellationToken ct)
    {
        if (request == null || string.IsNullOrEmpty(request.Format))
        {
            return BadRequest("Format is required (pdf or excel)");
        }

        var format = request.Format.ToLowerInvariant();
        var title = request.Title ?? "Export";

        try
        {
            return format switch
            {
                "pdf" => await ExportPdf(request, title, ct),
                "excel" => await ExportExcel(request, title, ct),
                _ => BadRequest($"Unsupported format: {format}. Use 'pdf' or 'excel'.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed for format {Format}", format);
            return StatusCode(StatusCodes.Status500InternalServerError, "Export failed");
        }
    }

    private async Task<IActionResult> ExportPdf(ExportRequest request, string title, CancellationToken ct)
    {
        // Dynamic data handling - would need to handle generic data in production
        // For now, assumes data is serializable to a known type

        var result = await _pdfExporter.ExportAsync(request.Data, title, ct);
        if (!result.Succeeded)
        {
            var errorMsg = result.MessageKey ?? result.Error?.Message ?? "Export failed";
            return BadRequest(errorMsg);
        }

        return File(result.Data!, MediaTypeNames.Application.Pdf, $"{title}.pdf");
    }

    private async Task<IActionResult> ExportExcel(ExportRequest request, string title, CancellationToken ct)
    {
        var result = await _excelExporter.ExportAsync(request.Data, title, ct);
        if (!result.Succeeded)
        {
            var errorMsg = result.MessageKey ?? result.Error?.Message ?? "Export failed";
            return BadRequest(errorMsg);
        }

        return File(result.Data!,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{title}.xlsx");
    }
}

public class ExportRequest
{
    public object[] Data { get; set; }
    public string Title { get; set; }
    public string Format { get; set; } // "pdf" or "excel"
}
