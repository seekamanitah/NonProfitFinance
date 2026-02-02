using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface IImportExportService
{
    // Import
    Task<ImportResult> ImportTransactionsFromCsvAsync(Stream csvStream, ImportMappingConfig mapping);
    Task<ImportResult> ImportTransactionsFromCsvAsync(Stream csvStream, ImportMappingConfig mapping, IProgress<ImportProgress>? progress);
    Task<ImportPreviewResult> PreviewImportAsync(Stream csvStream, ImportMappingConfig mapping);
    
    // Error Report
    byte[] GenerateErrorReportCsv(ImportResult result);
    
    // Export CSV
    Task<byte[]> ExportTransactionsToCsvAsync(TransactionFilterRequest filter);
    Task<byte[]> ExportCategoriesToCsvAsync();
    Task<byte[]> ExportDonorsToCsvAsync();
    Task<byte[]> ExportGrantsToCsvAsync();
    
    // Export Excel
    Task<byte[]> ExportTransactionsToExcelAsync(TransactionFilterRequest filter);
    Task<byte[]> ExportFinancialReportToExcelAsync(DateTime startDate, DateTime endDate);
    Task<byte[]> ExportDonorsToExcelAsync();
    Task<byte[]> ExportGrantsToExcelAsync();
    Task<byte[]> ExportToExcelAsync(TransactionFilterRequest? transactionFilter, bool includeCategories, bool includeDonors, bool includeGrants, bool includeFunds);
    
    // Templates
    byte[] GetTransactionImportTemplate();
    byte[] GetDonorImportTemplate();
}

public record ImportMappingConfig(
    int DateColumn,
    int AmountColumn,
    int DescriptionColumn,
    int? CategoryColumn = null,
    int? FundColumn = null,
    int? DonorColumn = null,
    int? GrantColumn = null,
    int? TypeColumn = null,
    int? PayeeColumn = null,
    int? TagsColumn = null,
    bool HasHeaderRow = true,
    string DateFormat = "yyyy-MM-dd"
);

public record ImportResult(
    bool Success,
    int TotalRows,
    int ImportedRows,
    int SkippedRows,
    List<ImportError> Errors,
    List<string> CreatedCategories
);

public record ImportProgress(
    int CurrentRow,
    int TotalRows,
    int ImportedCount,
    int SkippedCount,
    string? CurrentDescription
);

public record ImportPreviewResult(
    List<TransactionPreviewDto> Transactions,
    List<ImportError> ValidationErrors,
    List<string> NewCategories,
    int TotalRows
);

public record TransactionPreviewDto(
    int RowNumber,
    DateTime? Date,
    decimal? Amount,
    string? Description,
    string? Category,
    string? Fund,
    string? Donor,
    bool IsValid,
    List<string> Errors
);

public record ImportError(
    int RowNumber,
    string Column,
    string Message,
    string? OriginalData = null
);
