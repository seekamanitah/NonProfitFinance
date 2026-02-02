using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NonProfitFinance.DTOs;
using NonProfitFinance.Services;

namespace NonProfitFinance.Controllers;

/// <summary>
/// API controller for data import/export.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // HIGH-06 fix: Require authorization
public class ImportExportController : ControllerBase
{
    private readonly IImportExportService _importExportService;

    public ImportExportController(IImportExportService importExportService)
    {
        _importExportService = importExportService;
    }

    /// <summary>
    /// Preview CSV import before committing.
    /// </summary>
    [HttpPost("preview")]
    public async Task<ActionResult<ApiResponse<ImportPreviewResult>>> PreviewImport(
        IFormFile file,
        [FromQuery] int dateColumn = 0,
        [FromQuery] int amountColumn = 1,
        [FromQuery] int descriptionColumn = 2,
        [FromQuery] int? categoryColumn = null,
        [FromQuery] int? fundColumn = null,
        [FromQuery] int? donorColumn = null,
        [FromQuery] int? typeColumn = null,
        [FromQuery] bool hasHeaderRow = true,
        [FromQuery] string dateFormat = "yyyy-MM-dd")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse<ImportPreviewResult>(false, null, "No file provided"));

        var mapping = new ImportMappingConfig(
            dateColumn, amountColumn, descriptionColumn,
            categoryColumn, fundColumn, donorColumn, null, typeColumn,
            null, null, hasHeaderRow, dateFormat);

        using var stream = file.OpenReadStream();
        var result = await _importExportService.PreviewImportAsync(stream, mapping);

        return Ok(new ApiResponse<ImportPreviewResult>(true, result));
    }

    /// <summary>
    /// Import transactions from CSV file.
    /// </summary>
    [HttpPost("transactions")]
    public async Task<ActionResult<ApiResponse<ImportResult>>> ImportTransactions(
        IFormFile file,
        [FromQuery] int dateColumn = 0,
        [FromQuery] int amountColumn = 1,
        [FromQuery] int descriptionColumn = 2,
        [FromQuery] int? categoryColumn = null,
        [FromQuery] int? fundColumn = null,
        [FromQuery] int? donorColumn = null,
        [FromQuery] int? grantColumn = null,
        [FromQuery] int? typeColumn = null,
        [FromQuery] int? payeeColumn = null,
        [FromQuery] int? tagsColumn = null,
        [FromQuery] bool hasHeaderRow = true,
        [FromQuery] string dateFormat = "yyyy-MM-dd")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse<ImportResult>(false, null, "No file provided"));

        var mapping = new ImportMappingConfig(
            dateColumn, amountColumn, descriptionColumn,
            categoryColumn, fundColumn, donorColumn, grantColumn, typeColumn,
            payeeColumn, tagsColumn, hasHeaderRow, dateFormat);

        using var stream = file.OpenReadStream();
        var result = await _importExportService.ImportTransactionsFromCsvAsync(stream, mapping);

        return Ok(new ApiResponse<ImportResult>(result.Success, result,
            result.Success ? $"Imported {result.ImportedRows} transactions" : "Import completed with errors"));
    }

    /// <summary>
    /// Export transactions to CSV.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> ExportTransactions([FromQuery] TransactionFilterRequest filter)
    {
        var csv = await _importExportService.ExportTransactionsToCsvAsync(filter);
        return File(csv, "text/csv", $"transactions_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Export categories to CSV.
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> ExportCategories()
    {
        var csv = await _importExportService.ExportCategoriesToCsvAsync();
        return File(csv, "text/csv", $"categories_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Export donors to CSV.
    /// </summary>
    [HttpGet("donors")]
    public async Task<IActionResult> ExportDonors()
    {
        var csv = await _importExportService.ExportDonorsToCsvAsync();
        return File(csv, "text/csv", $"donors_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Export grants to CSV.
    /// </summary>
    [HttpGet("grants")]
    public async Task<IActionResult> ExportGrants()
    {
        var csv = await _importExportService.ExportGrantsToCsvAsync();
        return File(csv, "text/csv", $"grants_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Download transaction import template.
    /// </summary>
    [HttpGet("templates/transactions")]
    public IActionResult GetTransactionTemplate()
    {
        var template = _importExportService.GetTransactionImportTemplate();
        return File(template, "text/csv", "transaction_import_template.csv");
    }

    /// <summary>
    /// Download donor import template.
    /// </summary>
    [HttpGet("templates/donors")]
    public IActionResult GetDonorTemplate()
    {
        var template = _importExportService.GetDonorImportTemplate();
        return File(template, "text/csv", "donor_import_template.csv");
    }
}
