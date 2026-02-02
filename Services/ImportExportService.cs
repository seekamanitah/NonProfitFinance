using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class ImportExportService : IImportExportService
{
    private readonly ApplicationDbContext _context;
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
    private readonly IDonorService _donorService;
    private readonly IGrantService _grantService;
    private readonly IFundService _fundService;
    private readonly IAuditService _auditService; // MED-05 fix: Add audit service
    private readonly ILogger<ImportExportService> _logger;

    public ImportExportService(
        ApplicationDbContext context, 
        ITransactionService transactionService,
        ICategoryService categoryService,
        IDonorService donorService,
        IGrantService grantService,
        IFundService fundService,
        IAuditService auditService,
        ILogger<ImportExportService> logger)
    {
        _context = context;
        _transactionService = transactionService;
        _categoryService = categoryService;
        _donorService = donorService;
        _grantService = grantService;
        _fundService = fundService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ImportPreviewResult> PreviewImportAsync(Stream csvStream, ImportMappingConfig mapping)
    {
        var previews = new List<TransactionPreviewDto>();
        var errors = new List<ImportError>();
        var newCategories = new HashSet<string>();
        var existingCategories = await _context.Categories.Select(c => c.Name.ToLower()).ToListAsync();

        using var reader = new StreamReader(csvStream);
        var rowNumber = 0;
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            rowNumber++;

            if (rowNumber == 1 && mapping.HasHeaderRow)
                continue;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var columns = ParseCsvLine(line);
            var rowErrors = new List<string>();

            // Parse date
            DateTime? date = null;
            if (mapping.DateColumn < columns.Length)
            {
                if (DateTime.TryParseExact(columns[mapping.DateColumn], mapping.DateFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    date = parsedDate;
                }
                else if (DateTime.TryParse(columns[mapping.DateColumn], out parsedDate))
                {
                    date = parsedDate;
                }
                else
                {
                    rowErrors.Add("Invalid date format");
                }
            }

            // Parse amount
            decimal? amount = null;
            if (mapping.AmountColumn < columns.Length)
            {
                var amountStr = columns[mapping.AmountColumn].Replace("$", "").Replace(",", "").Trim();
                if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmount))
                {
                    amount = parsedAmount;
                }
                else
                {
                    rowErrors.Add("Invalid amount format");
                }
            }

            var description = mapping.DescriptionColumn < columns.Length ? columns[mapping.DescriptionColumn] : null;
            var category = mapping.CategoryColumn.HasValue && mapping.CategoryColumn.Value < columns.Length
                ? columns[mapping.CategoryColumn.Value] : null;
            var fund = mapping.FundColumn.HasValue && mapping.FundColumn.Value < columns.Length
                ? columns[mapping.FundColumn.Value] : null;
            var donor = mapping.DonorColumn.HasValue && mapping.DonorColumn.Value < columns.Length
                ? columns[mapping.DonorColumn.Value] : null;

            // Check for new categories
            if (!string.IsNullOrWhiteSpace(category) && !existingCategories.Contains(category.ToLower()))
            {
                newCategories.Add(category);
            }

            previews.Add(new TransactionPreviewDto(
                rowNumber,
                date,
                amount,
                description,
                category,
                fund,
                donor,
                rowErrors.Count == 0 && date.HasValue && amount.HasValue,
                rowErrors
            ));
        }

        return new ImportPreviewResult(previews, errors, newCategories.ToList(), rowNumber);
    }

    public async Task<ImportResult> ImportTransactionsFromCsvAsync(Stream csvStream, ImportMappingConfig mapping)
    {
        var errors = new List<ImportError>();
        var createdCategories = new List<string>();
        
        // Use GroupBy to handle duplicates - take first occurrence of each name
        var existingCategories = (await _context.Categories.ToListAsync())
            .GroupBy(c => c.Name.ToLower())
            .ToDictionary(g => g.Key, g => g.First().Id);
        
        var existingFunds = (await _context.Funds.ToListAsync())
            .GroupBy(f => f.Name.ToLower())
            .ToDictionary(g => g.Key, g => g.First().Id);
        
        var existingDonors = (await _context.Donors.ToListAsync())
            .GroupBy(d => d.Name.ToLower())
            .ToDictionary(g => g.Key, g => g.First().Id);

        var importedRows = 0;
        var skippedRows = 0;
        var totalRows = 0;

        using var reader = new StreamReader(csvStream);
        var rowNumber = 0;
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            rowNumber++;
            totalRows++;

            if (rowNumber == 1 && mapping.HasHeaderRow)
            {
                totalRows--;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                totalRows--;
                continue;
            }

            try
            {
                var columns = ParseCsvLine(line);

                // Parse required fields
                if (!DateTime.TryParse(columns[mapping.DateColumn], out var date))
                {
                    errors.Add(new ImportError(rowNumber, "Date", "Invalid date format"));
                    skippedRows++;
                    continue;
                }

                var amountStr = columns[mapping.AmountColumn].Replace("$", "").Replace(",", "").Trim();
                if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                {
                    errors.Add(new ImportError(rowNumber, "Amount", "Invalid amount format"));
                    skippedRows++;
                    continue;
                }

                var description = mapping.DescriptionColumn < columns.Length ? columns[mapping.DescriptionColumn] : "";

                // Determine transaction type
                var type = TransactionType.Expense;
                if (mapping.TypeColumn.HasValue && mapping.TypeColumn.Value < columns.Length)
                {
                    var typeStr = columns[mapping.TypeColumn.Value].ToLower();
                    type = typeStr switch
                    {
                        "income" or "deposit" or "credit" => TransactionType.Income,
                        "expense" or "withdrawal" or "debit" => TransactionType.Expense,
                        "transfer" => TransactionType.Transfer,
                        _ => amount >= 0 ? TransactionType.Income : TransactionType.Expense
                    };
                }
                else
                {
                    type = amount >= 0 ? TransactionType.Income : TransactionType.Expense;
                }

                // Handle category
                var categoryId = 1; // Default to first category
                if (mapping.CategoryColumn.HasValue && mapping.CategoryColumn.Value < columns.Length)
                {
                    var categoryName = columns[mapping.CategoryColumn.Value].Trim();
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        if (existingCategories.TryGetValue(categoryName.ToLower(), out var catId))
                        {
                            categoryId = catId;
                        }
                        else
                        {
                            // Create new category
                            var newCategory = new Category
                            {
                                Name = categoryName,
                                Type = type == TransactionType.Income ? CategoryType.Income : CategoryType.Expense
                            };
                            _context.Categories.Add(newCategory);
                            await _context.SaveChangesAsync();
                            categoryId = newCategory.Id;
                            existingCategories[categoryName.ToLower()] = categoryId;
                            createdCategories.Add(categoryName);
                        }
                    }
                }

                // Handle fund
                int? fundId = null;
                if (mapping.FundColumn.HasValue && mapping.FundColumn.Value < columns.Length)
                {
                    var fundName = columns[mapping.FundColumn.Value].Trim();
                    if (!string.IsNullOrEmpty(fundName) && existingFunds.TryGetValue(fundName.ToLower(), out var fId))
                    {
                        fundId = fId;
                    }
                }

                // Handle donor
                int? donorId = null;
                if (mapping.DonorColumn.HasValue && mapping.DonorColumn.Value < columns.Length)
                {
                    var donorName = columns[mapping.DonorColumn.Value].Trim();
                    if (!string.IsNullOrEmpty(donorName))
                    {
                        if (existingDonors.TryGetValue(donorName.ToLower(), out var dId))
                        {
                            donorId = dId;
                        }
                        else
                        {
                            // Create new donor
                            var newDonor = new Donor { Name = donorName };
                            _context.Donors.Add(newDonor);
                            await _context.SaveChangesAsync();
                            donorId = newDonor.Id;
                            existingDonors[donorName.ToLower()] = donorId.Value;
                        }
                    }
                }

                var payee = mapping.PayeeColumn.HasValue && mapping.PayeeColumn.Value < columns.Length
                    ? columns[mapping.PayeeColumn.Value] : null;
                var tags = mapping.TagsColumn.HasValue && mapping.TagsColumn.Value < columns.Length
                    ? columns[mapping.TagsColumn.Value] : null;

                await _transactionService.CreateAsync(new CreateTransactionRequest(
                    date,
                    Math.Abs(amount),
                    description,
                    type,
                    categoryId,
                    FundType.Unrestricted,
                    fundId,
                    null, // ToFundId
                    donorId,
                    null, // GrantId
                    payee,
                    tags
                ));

                importedRows++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError(rowNumber, "Row", ex.Message));
                skippedRows++;
            }
        }

        // MED-05 fix: Audit log for bulk import
        await _auditService.LogAsync(
            AuditAction.Create,
            "BulkImport",
            0,
            $"Imported {importedRows} transactions from CSV (Total: {totalRows}, Skipped: {skippedRows}, Errors: {errors.Count})",
            newValues: new { 
                TotalRows = totalRows, 
                ImportedRows = importedRows, 
                SkippedRows = skippedRows,
                ErrorCount = errors.Count,
                CreatedCategories = createdCategories
            }
        );

        _logger.LogInformation(
            "CSV Import completed: {ImportedRows} imported, {SkippedRows} skipped, {ErrorCount} errors",
            importedRows, skippedRows, errors.Count);

        return new ImportResult(
            errors.Count == 0,
            totalRows,
            importedRows,
            skippedRows,
            errors,
            createdCategories
        );
    }

    public async Task<byte[]> ExportTransactionsToCsvAsync(TransactionFilterRequest filter)
    {
        var result = await _transactionService.GetAllAsync(filter with { PageSize = int.MaxValue });
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Date,Amount,Description,Type,Category,Fund,Donor,Grant,Payee,Tags,Reference,Reconciled");

        foreach (var t in result.Items)
        {
            sb.AppendLine($"{t.Date:yyyy-MM-dd},{t.Amount},{EscapeCsv(t.Description)},{t.Type},{EscapeCsv(t.CategoryName)},{EscapeCsv(t.FundName)},{EscapeCsv(t.DonorName)},{EscapeCsv(t.GrantName)},{EscapeCsv(t.Payee)},{EscapeCsv(t.Tags)},{EscapeCsv(t.ReferenceNumber)},{t.IsReconciled}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportCategoriesToCsvAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Parent)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.SortOrder)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID,Name,Type,Parent,Description,Color,BudgetLimit,Archived");

        foreach (var c in categories)
        {
            sb.AppendLine($"{c.Id},{EscapeCsv(c.Name)},{c.Type},{EscapeCsv(c.Parent?.Name)},{EscapeCsv(c.Description)},{c.Color},{c.BudgetLimit},{c.IsArchived}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportDonorsToCsvAsync()
    {
        var donors = await _context.Donors.OrderBy(d => d.Name).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID,Name,Type,Email,Phone,Address,TotalContributions,FirstContribution,LastContribution,Anonymous,Active");

        foreach (var d in donors)
        {
            sb.AppendLine($"{d.Id},{EscapeCsv(d.Name)},{d.Type},{EscapeCsv(d.Email)},{EscapeCsv(d.Phone)},{EscapeCsv(d.Address)},{d.TotalContributions},{d.FirstContributionDate:yyyy-MM-dd},{d.LastContributionDate:yyyy-MM-dd},{d.IsAnonymous},{d.IsActive}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportGrantsToCsvAsync()
    {
        var grants = await _context.Grants.OrderByDescending(g => g.StartDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID,Name,Grantor,Amount,AmountUsed,Remaining,StartDate,EndDate,Status,GrantNumber,Restrictions");

        foreach (var g in grants)
        {
            sb.AppendLine($"{g.Id},{EscapeCsv(g.Name)},{EscapeCsv(g.GrantorName)},{g.Amount},{g.AmountUsed},{g.RemainingBalance},{g.StartDate:yyyy-MM-dd},{g.EndDate:yyyy-MM-dd},{g.Status},{EscapeCsv(g.GrantNumber)},{EscapeCsv(g.Restrictions)}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] GetTransactionImportTemplate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Date,Amount,Description,Type,Category,Fund,Donor,Grant ID,Payee,Tags");
        sb.AppendLine("2024-01-15,500.00,Monthly donation,Income,Individual/Small Business,General Operating Fund,John Smith,,Donor,,");
        sb.AppendLine("2024-01-16,-150.00,Office supplies,Expense,Office Supplies,General Operating Fund,,,Office Depot,supplies");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] GetDonorImportTemplate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name,Type,Email,Phone,Address,Notes,Anonymous");
        sb.AppendLine("John Smith,Individual,john@example.com,555-1234,123 Main St,Regular monthly donor,false");
        sb.AppendLine("ABC Corporation,Corporate,contact@abc.com,555-5678,456 Business Ave,Annual sponsor,false");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = new StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString().Trim());
        return result.ToArray();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    // Excel Export Methods

    public async Task<byte[]> ExportTransactionsToExcelAsync(TransactionFilterRequest filter)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Fund)
            .Include(t => t.Donor)
            .Include(t => t.Grant)
            .Where(t => (filter.StartDate == null || t.Date >= filter.StartDate) &&
                        (filter.EndDate == null || t.Date <= filter.EndDate))
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        // Headers
        var headers = new[] { "Date", "Description", "Type", "Category", "Amount", "Fund", "Donor", "Grant", "Payee", "Tags" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkRed;
            worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        // Data
        var row = 2;
        foreach (var t in transactions)
        {
            worksheet.Cell(row, 1).Value = t.Date;
            worksheet.Cell(row, 2).Value = t.Description ?? "";
            worksheet.Cell(row, 3).Value = t.Type.ToString();
            worksheet.Cell(row, 4).Value = t.Category?.Name ?? "";
            worksheet.Cell(row, 5).Value = t.Type == TransactionType.Income ? t.Amount : -t.Amount;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 6).Value = t.Fund?.Name ?? "";
            worksheet.Cell(row, 7).Value = t.Donor?.Name ?? "";
            worksheet.Cell(row, 8).Value = t.Grant?.Name ?? "";
            worksheet.Cell(row, 9).Value = t.Payee ?? "";
            worksheet.Cell(row, 10).Value = t.Tags ?? "";

            // Color code by type
            if (t.Type == TransactionType.Income)
            {
                worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;
            }
            else
            {
                worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;
            }

            row++;
        }

        // Totals
        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        
        row++;
        worksheet.Cell(row, 4).Value = "Total Income:";
        worksheet.Cell(row, 4).Style.Font.Bold = true;
        worksheet.Cell(row, 5).Value = totalIncome;
        worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;
        worksheet.Cell(row, 5).Style.Font.Bold = true;

        row++;
        worksheet.Cell(row, 4).Value = "Total Expenses:";
        worksheet.Cell(row, 4).Style.Font.Bold = true;
        worksheet.Cell(row, 5).Value = -totalExpense;
        worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;
        worksheet.Cell(row, 5).Style.Font.Bold = true;

        row++;
        worksheet.Cell(row, 4).Value = "Net:";
        worksheet.Cell(row, 4).Style.Font.Bold = true;
        worksheet.Cell(row, 5).Value = totalIncome - totalExpense;
        worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 5).Style.Font.Bold = true;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportFinancialReportToExcelAsync(DateTime startDate, DateTime endDate)
    {
        using var workbook = new XLWorkbook();

        // Summary Sheet
        var summarySheet = workbook.Worksheets.Add("Summary");
        
        var totalIncome = await _context.Transactions
            .Where(t => t.Type == TransactionType.Income && t.Date >= startDate && t.Date <= endDate)
            .SumAsync(t => t.Amount);
        var totalExpense = await _context.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startDate && t.Date <= endDate)
            .SumAsync(t => t.Amount);

        summarySheet.Cell("A1").Value = "Financial Summary Report";
        summarySheet.Cell("A1").Style.Font.Bold = true;
        summarySheet.Cell("A1").Style.Font.FontSize = 16;
        
        summarySheet.Cell("A2").Value = $"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}";
        
        summarySheet.Cell("A4").Value = "Total Income";
        summarySheet.Cell("B4").Value = totalIncome;
        summarySheet.Cell("B4").Style.NumberFormat.Format = "$#,##0.00";
        summarySheet.Cell("B4").Style.Font.FontColor = XLColor.DarkGreen;
        
        summarySheet.Cell("A5").Value = "Total Expenses";
        summarySheet.Cell("B5").Value = totalExpense;
        summarySheet.Cell("B5").Style.NumberFormat.Format = "$#,##0.00";
        summarySheet.Cell("B5").Style.Font.FontColor = XLColor.DarkRed;
        
        summarySheet.Cell("A6").Value = "Net Income";
        summarySheet.Cell("B6").Value = totalIncome - totalExpense;
        summarySheet.Cell("B6").Style.NumberFormat.Format = "$#,##0.00";
        summarySheet.Cell("B6").Style.Font.Bold = true;

        // Income by Category Sheet
        var incomeSheet = workbook.Worksheets.Add("Income by Category");
        var incomeByCategory = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Type == TransactionType.Income && t.Date >= startDate && t.Date <= endDate)
            .GroupBy(t => t.Category!.Name)
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();

        incomeSheet.Cell("A1").Value = "Category";
        incomeSheet.Cell("B1").Value = "Amount";
        incomeSheet.Cell("C1").Value = "% of Total";
        incomeSheet.Row(1).Style.Font.Bold = true;
        incomeSheet.Row(1).Style.Fill.BackgroundColor = XLColor.DarkGreen;
        incomeSheet.Row(1).Style.Font.FontColor = XLColor.White;

        var incomeRow = 2;
        foreach (var item in incomeByCategory)
        {
            incomeSheet.Cell(incomeRow, 1).Value = item.Category;
            incomeSheet.Cell(incomeRow, 2).Value = item.Amount;
            incomeSheet.Cell(incomeRow, 2).Style.NumberFormat.Format = "$#,##0.00";
            incomeSheet.Cell(incomeRow, 3).Value = totalIncome > 0 ? item.Amount / totalIncome : 0;
            incomeSheet.Cell(incomeRow, 3).Style.NumberFormat.Format = "0.0%";
            incomeRow++;
        }
        incomeSheet.Columns().AdjustToContents();

        // Expense by Category Sheet
        var expenseSheet = workbook.Worksheets.Add("Expenses by Category");
        var expenseByCategory = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startDate && t.Date <= endDate)
            .GroupBy(t => t.Category!.Name)
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();

        expenseSheet.Cell("A1").Value = "Category";
        expenseSheet.Cell("B1").Value = "Amount";
        expenseSheet.Cell("C1").Value = "% of Total";
        expenseSheet.Row(1).Style.Font.Bold = true;
        expenseSheet.Row(1).Style.Fill.BackgroundColor = XLColor.DarkRed;
        expenseSheet.Row(1).Style.Font.FontColor = XLColor.White;

        var expenseRow = 2;
        foreach (var item in expenseByCategory)
        {
            expenseSheet.Cell(expenseRow, 1).Value = item.Category;
            expenseSheet.Cell(expenseRow, 2).Value = item.Amount;
            expenseSheet.Cell(expenseRow, 2).Style.NumberFormat.Format = "$#,##0.00";
            expenseSheet.Cell(expenseRow, 3).Value = totalExpense > 0 ? item.Amount / totalExpense : 0;
            expenseSheet.Cell(expenseRow, 3).Style.NumberFormat.Format = "0.0%";
            expenseRow++;
        }
        expenseSheet.Columns().AdjustToContents();

        summarySheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportDonorsToExcelAsync()
    {
        var donors = await _context.Donors.OrderByDescending(d => d.TotalContributions).ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Donors");

        var headers = new[] { "Name", "Type", "Email", "Phone", "Total Contributions", "First Contribution", "Last Contribution", "Active" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkRed;
            worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        var row = 2;
        foreach (var d in donors)
        {
            worksheet.Cell(row, 1).Value = d.Name;
            worksheet.Cell(row, 2).Value = d.Type.ToString();
            worksheet.Cell(row, 3).Value = d.Email ?? "";
            worksheet.Cell(row, 4).Value = d.Phone ?? "";
            worksheet.Cell(row, 5).Value = d.TotalContributions;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 6).Value = d.FirstContributionDate;
            worksheet.Cell(row, 7).Value = d.LastContributionDate;
            worksheet.Cell(row, 8).Value = d.IsActive ? "Yes" : "No";
            row++;
        }

        // Total
        row++;
        worksheet.Cell(row, 4).Value = "Total:";
        worksheet.Cell(row, 4).Style.Font.Bold = true;
        worksheet.Cell(row, 5).Value = donors.Sum(d => d.TotalContributions);
        worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 5).Style.Font.Bold = true;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportGrantsToExcelAsync()
    {
        var grants = await _context.Grants.OrderByDescending(g => g.StartDate).ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Grants");

        var headers = new[] { "Name", "Grantor", "Status", "Amount", "Used", "Remaining", "Start Date", "End Date", "Grant #" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkRed;
            worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        var row = 2;
        foreach (var g in grants)
        {
            worksheet.Cell(row, 1).Value = g.Name;
            worksheet.Cell(row, 2).Value = g.GrantorName;
            worksheet.Cell(row, 3).Value = g.Status.ToString();
            worksheet.Cell(row, 4).Value = g.Amount;
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 5).Value = g.AmountUsed;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 6).Value = g.RemainingBalance;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 7).Value = g.StartDate;
            worksheet.Cell(row, 8).Value = g.EndDate;
            worksheet.Cell(row, 9).Value = g.GrantNumber ?? "";

            // Highlight based on status
            if (g.Status == GrantStatus.Expired)
            {
                worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightPink;
            }
            else if (g.Status == GrantStatus.Active)
            {
                worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightGreen;
            }

            row++;
        }

        // Totals
        row++;
        worksheet.Cell(row, 3).Value = "Totals:";
        worksheet.Cell(row, 3).Style.Font.Bold = true;
        worksheet.Cell(row, 4).Value = grants.Sum(g => g.Amount);
        worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 4).Style.Font.Bold = true;
        worksheet.Cell(row, 5).Value = grants.Sum(g => g.AmountUsed);
        worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 5).Style.Font.Bold = true;
        worksheet.Cell(row, 6).Value = grants.Sum(g => g.RemainingBalance);
        worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 6).Style.Font.Bold = true;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToExcelAsync(
        TransactionFilterRequest? transactionFilter,
        bool includeCategories,
        bool includeDonors,
        bool includeGrants,
        bool includeFunds)
    {
        using var workbook = new XLWorkbook();

        // Add Transactions sheet if requested
        if (transactionFilter != null)
        {
            var transactions = await _transactionService.GetAllAsync(transactionFilter);
            var worksheet = workbook.Worksheets.Add("Transactions");

            // Headers
            string[] headers = { "Date", "Description", "Category", "Type", "Amount", "Fund", "Donor", "Grant", "Payee", "Tags" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkRed;
                worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var t in transactions.Items)
            {
                worksheet.Cell(row, 1).Value = t.Date;
                worksheet.Cell(row, 2).Value = t.Description ?? "";
                worksheet.Cell(row, 3).Value = t.CategoryName ?? "";
                worksheet.Cell(row, 4).Value = t.Type.ToString();
                worksheet.Cell(row, 5).Value = t.Type == TransactionType.Income ? t.Amount : -t.Amount;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 6).Value = t.FundName ?? "";
                worksheet.Cell(row, 7).Value = t.DonorName ?? "";
                worksheet.Cell(row, 8).Value = t.GrantName ?? "";
                worksheet.Cell(row, 9).Value = t.Payee ?? "";
                worksheet.Cell(row, 10).Value = t.Tags ?? "";

                if (t.Type == TransactionType.Income)
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.Green;
                }
                else
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();
        }

        // Add Categories sheet if requested
        if (includeCategories)
        {
            var categories = await _categoryService.GetAllAsync();
            var worksheet = workbook.Worksheets.Add("Categories");

            string[] headers = { "Name", "Type", "Description", "Budget Limit", "Parent", "Archived" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkRed;
                worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var c in categories)
            {
                worksheet.Cell(row, 1).Value = c.Name;
                worksheet.Cell(row, 2).Value = c.Type.ToString();
                worksheet.Cell(row, 3).Value = c.Description ?? "";
                worksheet.Cell(row, 4).Value = c.BudgetLimit ?? 0;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 5).Value = c.ParentName ?? "";
                worksheet.Cell(row, 6).Value = c.IsArchived ? "Yes" : "No";
                row++;
            }

            worksheet.Columns().AdjustToContents();
        }

        // Add Donors sheet if requested
        if (includeDonors)
        {
            var donors = await _donorService.GetAllAsync();
            var worksheet = workbook.Worksheets.Add("Donors");

            string[] headers = { "Name", "Type", "Email", "Phone", "Total Contributions", "Active" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkRed;
                worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var d in donors)
            {
                worksheet.Cell(row, 1).Value = d.Name;
                worksheet.Cell(row, 2).Value = d.Type.ToString();
                worksheet.Cell(row, 3).Value = d.Email ?? "";
                worksheet.Cell(row, 4).Value = d.Phone ?? "";
                worksheet.Cell(row, 5).Value = d.TotalContributions;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 6).Value = d.IsActive ? "Yes" : "No";
                row++;
            }

            worksheet.Columns().AdjustToContents();
        }

        // Add Grants sheet if requested
        if (includeGrants)
        {
            var grants = await _grantService.GetAllAsync(null);
            var worksheet = workbook.Worksheets.Add("Grants");

            string[] headers = { "Name", "Grantor", "Status", "Amount", "Used", "Remaining", "Start Date", "End Date", "Grant #" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkRed;
                worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var g in grants)
            {
                worksheet.Cell(row, 1).Value = g.Name;
                worksheet.Cell(row, 2).Value = g.GrantorName;
                worksheet.Cell(row, 3).Value = g.Status.ToString();
                worksheet.Cell(row, 4).Value = g.Amount;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 5).Value = g.AmountUsed;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 6).Value = g.RemainingBalance;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 7).Value = g.StartDate;
                worksheet.Cell(row, 8).Value = g.EndDate;
                worksheet.Cell(row, 9).Value = g.GrantNumber ?? "";
                row++;
            }

            worksheet.Columns().AdjustToContents();
        }

        // Add Funds sheet if requested
        if (includeFunds)
        {
            var funds = await _fundService.GetAllAsync();
            var worksheet = workbook.Worksheets.Add("Funds");

            string[] headers = { "Name", "Type", "Balance", "Description", "Active" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.DarkRed;
                worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var row = 2;
            foreach (var f in funds)
            {
                worksheet.Cell(row, 1).Value = f.Name;
                worksheet.Cell(row, 2).Value = f.Type.ToString();
                worksheet.Cell(row, 3).Value = f.Balance;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 4).Value = f.Description ?? "";
                worksheet.Cell(row, 5).Value = f.IsActive ? "Yes" : "No";
                row++;
            }

            worksheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
