using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NonProfitFinance.Services;

public class Form990Service : IForm990Service
{
    private readonly ApplicationDbContext _context;

    // IRS thresholds
    private const decimal Form990NThreshold = 50_000m;
    private const decimal Form990EZRevenueThreshold = 200_000m;
    private const decimal Form990EZAssetThreshold = 500_000m;

    public Form990Service(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Form990DataDto> GetForm990DataAsync(int fiscalYear)
    {
        var startDate = new DateTime(fiscalYear, 1, 1);
        var endDate = new DateTime(fiscalYear, 12, 31);

        var partI = await GetPartIDataAsync(startDate, endDate);
        var partVIII = await GetPartVIIIDataAsync(startDate, endDate);
        var partIX = await GetPartIXDataAsync(startDate, endDate);
        var formType = await GetRequiredFormTypeAsync(fiscalYear);

        return new Form990DataDto(
            fiscalYear,
            formType,
            partI,
            partVIII,
            partIX,
            partI.TotalRevenue,
            partI.TotalExpenses,
            partI.NetAssets
        );
    }

    public async Task<decimal> GetGrossRevenueAsync(int fiscalYear)
    {
        var startDate = new DateTime(fiscalYear, 1, 1);
        var endDate = new DateTime(fiscalYear, 12, 31);

        return await _context.Transactions
            .Where(t => t.Type == TransactionType.Income && t.Date >= startDate && t.Date <= endDate)
            .SumAsync(t => t.Amount);
    }

    public async Task<Form990Type> GetRequiredFormTypeAsync(int fiscalYear)
    {
        var grossRevenue = await GetGrossRevenueAsync(fiscalYear);
        var totalAssets = await GetTotalAssetsAsync();

        if (grossRevenue <= Form990NThreshold)
            return Form990Type.Form990N;

        if (grossRevenue < Form990EZRevenueThreshold && totalAssets < Form990EZAssetThreshold)
            return Form990Type.Form990EZ;

        return Form990Type.Form990;
    }

    public async Task<byte[]> ExportForm990ToPdfAsync(int fiscalYear)
    {
        var data = await GetForm990DataAsync(fiscalYear);

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text($"Form 990 Worksheet - Fiscal Year {fiscalYear}")
                        .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"Required Form: {data.RequiredFormType}")
                        .FontSize(11).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                });

                page.Content().PaddingVertical(15).Column(column =>
                {
                    // Part I Summary
                    column.Item().Text("Part I - Summary").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(100);
                        });

                        AddPartILine(table, "6", "Gross receipts", data.PartI.GrossReceipts);
                        AddPartILine(table, "8", "Contributions and grants", data.PartI.Contributions);
                        AddPartILine(table, "9", "Program service revenue", data.PartI.ProgramServiceRevenue);
                        AddPartILine(table, "10", "Investment income", data.PartI.InvestmentIncome);
                        AddPartILine(table, "11", "Other revenue", data.PartI.OtherRevenue);
                        AddPartILine(table, "12", "Total revenue", data.PartI.TotalRevenue, true);
                        AddPartILine(table, "13", "Grants and similar amounts paid", data.PartI.GrantsAndSimilar);
                        AddPartILine(table, "15", "Salaries, compensation", data.PartI.Salaries);
                        AddPartILine(table, "17", "Total expenses", data.PartI.TotalExpenses, true);
                        AddPartILine(table, "18", "Revenue minus expenses", data.PartI.RevenueMinusExpenses, true);
                        AddPartILine(table, "19", "Total assets", data.PartI.TotalAssets);
                        AddPartILine(table, "20", "Total liabilities", data.PartI.TotalLiabilities);
                        AddPartILine(table, "21", "Net assets", data.PartI.NetAssets, true);
                    });

                    column.Item().PaddingTop(20);

                    // Part VIII Summary
                    column.Item().Text("Part VIII - Statement of Revenue").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(100);
                        });

                        table.Cell().Padding(4).Text("Government grants (1e)");
                        table.Cell().Padding(4).Text($"{data.PartVIII.GovernmentGrants:C}").AlignRight();

                        table.Cell().Padding(4).Text("Other contributions (1f)");
                        table.Cell().Padding(4).Text($"{data.PartVIII.OtherContributions:C}").AlignRight();

                        table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Total contributions (1h)").Bold();
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text($"{data.PartVIII.TotalContributions:C}").Bold().AlignRight();

                        table.Cell().Padding(4).Text("Program service revenue (2g)");
                        table.Cell().Padding(4).Text($"{data.PartVIII.TotalProgramServiceRevenue:C}").AlignRight();

                        table.Cell().Padding(4).Text("Investment income (3+4)");
                        table.Cell().Padding(4).Text($"{data.PartVIII.InvestmentIncomeInterest + data.PartVIII.DividendsAndInterest:C}").AlignRight();

                        table.Cell().Background(Colors.Blue.Lighten4).Padding(4).Text("Total Revenue (12)").Bold();
                        table.Cell().Background(Colors.Blue.Lighten4).Padding(4).Text($"{data.PartVIII.TotalRevenue:C}").Bold().AlignRight();
                    });

                    // Notes
                    column.Item().PaddingTop(20);
                    column.Item().Background(Colors.Yellow.Lighten4).Padding(10).Column(noteCol =>
                    {
                        noteCol.Item().Text("Notes:").FontSize(10).Bold();
                        noteCol.Item().Text("• This worksheet is for planning purposes only").FontSize(9);
                        noteCol.Item().Text("• Consult with a tax professional for actual filing").FontSize(9);
                        noteCol.Item().Text("• Some line items may require manual allocation").FontSize(9);
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated: ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.Now.ToString("MMMM dd, yyyy")).FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" | Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8);
                    x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportForm990ToExcelAsync(int fiscalYear)
    {
        var data = await GetForm990DataAsync(fiscalYear);

        using var workbook = new XLWorkbook();

        // Summary Sheet
        var summarySheet = workbook.Worksheets.Add("Form 990 Summary");
        summarySheet.Cell("A1").Value = $"Form 990 Worksheet - Fiscal Year {fiscalYear}";
        summarySheet.Cell("A1").Style.Font.Bold = true;
        summarySheet.Cell("A1").Style.Font.FontSize = 14;
        
        summarySheet.Cell("A2").Value = $"Required Form Type: {data.RequiredFormType}";
        summarySheet.Cell("A3").Value = $"Generated: {DateTime.Now:MMMM dd, yyyy}";

        // Part I
        var row = 5;
        summarySheet.Cell(row, 1).Value = "Part I - Summary";
        summarySheet.Cell(row, 1).Style.Font.Bold = true;
        summarySheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        row++;

        AddExcelLine(summarySheet, ref row, "Line 6", "Gross receipts", data.PartI.GrossReceipts);
        AddExcelLine(summarySheet, ref row, "Line 8", "Contributions and grants", data.PartI.Contributions);
        AddExcelLine(summarySheet, ref row, "Line 9", "Program service revenue", data.PartI.ProgramServiceRevenue);
        AddExcelLine(summarySheet, ref row, "Line 10", "Investment income", data.PartI.InvestmentIncome);
        AddExcelLine(summarySheet, ref row, "Line 11", "Other revenue", data.PartI.OtherRevenue);
        AddExcelLine(summarySheet, ref row, "Line 12", "Total revenue", data.PartI.TotalRevenue, true);
        AddExcelLine(summarySheet, ref row, "Line 13", "Grants paid", data.PartI.GrantsAndSimilar);
        AddExcelLine(summarySheet, ref row, "Line 15", "Salaries", data.PartI.Salaries);
        AddExcelLine(summarySheet, ref row, "Line 17", "Total expenses", data.PartI.TotalExpenses, true);
        AddExcelLine(summarySheet, ref row, "Line 18", "Net", data.PartI.RevenueMinusExpenses, true);
        AddExcelLine(summarySheet, ref row, "Line 19", "Total assets", data.PartI.TotalAssets);
        AddExcelLine(summarySheet, ref row, "Line 20", "Total liabilities", data.PartI.TotalLiabilities);
        AddExcelLine(summarySheet, ref row, "Line 21", "Net assets", data.PartI.NetAssets, true);

        summarySheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // Helper methods
    private async Task<Form990PartI> GetPartIDataAsync(DateTime startDate, DateTime endDate)
    {
        var income = await _context.Transactions
            .Where(t => t.Type == TransactionType.Income && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        var expenses = await _context.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        var totalIncome = income.Sum(t => t.Amount);
        var totalExpenses = expenses.Sum(t => t.Amount);
        var totalAssets = await GetTotalAssetsAsync();

        // Categorize income (simplified - would need category mapping for real implementation)
        var contributions = income.Sum(t => t.Amount); // Simplified
        var programRevenue = 0m;
        var investmentIncome = 0m;
        var otherRevenue = 0m;

        // Categorize expenses (simplified)
        var salaries = 0m;
        var grants = 0m;
        var benefits = 0m;
        var otherExpenses = totalExpenses;

        return new Form990PartI(
            totalIncome,           // Gross receipts
            contributions,         // Contributions
            programRevenue,        // Program service revenue
            investmentIncome,      // Investment income
            otherRevenue,          // Other revenue
            totalIncome,           // Total revenue
            grants,                // Grants paid
            benefits,              // Benefits
            salaries,              // Salaries
            otherExpenses,         // Other expenses
            totalExpenses,         // Total expenses
            totalIncome - totalExpenses, // Net
            totalAssets,           // Total assets
            0,                     // Total liabilities (would need balance sheet)
            totalAssets            // Net assets
        );
    }

    private async Task<Form990PartVIII> GetPartVIIIDataAsync(DateTime startDate, DateTime endDate)
    {
        var income = await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Grant)
            .Where(t => t.Type == TransactionType.Income && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        var governmentGrants = income.Where(t => t.Grant != null).Sum(t => t.Amount);
        var otherContributions = income.Where(t => t.Grant == null).Sum(t => t.Amount);
        var totalRevenue = income.Sum(t => t.Amount);

        return new Form990PartVIII(
            0, 0, 0, 0,                    // Federated, membership, fundraising, related
            governmentGrants,              // Government grants
            otherContributions,            // Other contributions
            otherContributions + governmentGrants, // Total contributions
            new List<ProgramServiceRevenueItem>(),
            0,                             // Program service revenue
            0, 0, 0, 0, 0, 0, 0,          // Other revenue lines
            totalRevenue
        );
    }

    private async Task<Form990PartIX> GetPartIXDataAsync(DateTime startDate, DateTime endDate)
    {
        var expenses = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        var totalExpenses = expenses.Sum(t => t.Amount);

        // Simplified - all expenses as program services
        var expenseLines = new List<Form990ExpenseLine>
        {
            new(25, "Other expenses", totalExpenses, totalExpenses, 0, 0)
        };

        return new Form990PartIX(
            expenseLines,
            totalExpenses,
            totalExpenses, // Program services
            0,             // Management
            0              // Fundraising
        );
    }

    private async Task<decimal> GetTotalAssetsAsync()
    {
        // Calculate from fund balances
        var funds = await _context.Funds.ToListAsync();
        return funds.Sum(f => f.Balance);
    }

    private static void AddPartILine(TableDescriptor table, string line, string description, decimal amount, bool highlight = false)
    {
        var bgColor = highlight ? Colors.Grey.Lighten3 : Colors.White;
        
        table.Cell().Background(bgColor).Padding(3).Text($"Line {line}").FontSize(8);
        table.Cell().Background(bgColor).Padding(3).Text(description);
        table.Cell().Background(bgColor).Padding(3).Text($"{amount:C}").AlignRight();
    }

    private static void AddExcelLine(IXLWorksheet sheet, ref int row, string line, string description, decimal amount, bool highlight = false)
    {
        sheet.Cell(row, 1).Value = line;
        sheet.Cell(row, 2).Value = description;
        sheet.Cell(row, 3).Value = amount;
        sheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
        
        if (highlight)
        {
            sheet.Row(row).Style.Font.Bold = true;
            sheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        
        row++;
    }
}
