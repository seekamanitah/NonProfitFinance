using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public class PdfExportService : IPdfExportService
{
    private readonly IReportService _reportService;
    private readonly ITransactionService _transactionService;
    private readonly IDonorService _donorService;
    private readonly IGrantService _grantService;
    private readonly ICategoryService _categoryService;
    private readonly IDataIntegrityService _dataIntegrityService;

    public PdfExportService(
        IReportService reportService,
        ITransactionService transactionService,
        IDonorService donorService,
        IGrantService grantService,
        ICategoryService categoryService,
        IDataIntegrityService dataIntegrityService)
    {
        _reportService = reportService;
        _transactionService = transactionService;
        _donorService = donorService;
        _grantService = grantService;
        _categoryService = categoryService;
        _dataIntegrityService = dataIntegrityService;
        
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateFinancialReportAsync(ReportRequest request)
    {
        var filter = new ReportFilterRequest(request.StartDate, request.EndDate);
        var summary = await _reportService.GetIncomeExpenseSummaryAsync(filter);
        var trends = await _reportService.GetTrendDataAsync(request.StartDate, request.EndDate);

        var theme = GetThemeColors(request.Theme);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(theme.TextColor));

                page.Header().Element(c => ComposeHeader(c, "Financial Report", request, theme));
                
                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    // Summary Section - Use summary data, not metrics
                    column.Item().Element(c => ComposeSummarySection(c, summary, theme));

                    // Income Section
                    column.Item().Element(c => ComposeCategorySection(c, "Income by Category", 
                        summary.IncomeByCategory, theme, isIncome: true));

                    // Expense Section
                    column.Item().Element(c => ComposeCategorySection(c, "Expenses by Category", 
                        summary.ExpensesByCategory, theme, isIncome: false));

                    // Monthly Trend
                    if (request.IncludeDetails)
                    {
                        column.Item().Element(c => ComposeTrendSection(c, trends, theme));
                    }
                });

                page.Footer().Element(c => ComposeFooter(c, theme));
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateTransactionReportAsync(TransactionFilterRequest filter)
    {
        var result = await _transactionService.GetAllAsync(filter);
        var transactions = result.Items;
        var theme = GetThemeColors(ReportTheme.Modern);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("Transaction Report").FontSize(18).Bold().FontColor(theme.PrimaryColor);
                    col.Item().Text($"Period: {filter.StartDate:MMM dd, yyyy} - {filter.EndDate:MMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(80);  // Date
                        columns.RelativeColumn(3);   // Description
                        columns.RelativeColumn(2);   // Category
                        columns.RelativeColumn(1.5f); // Fund
                        columns.RelativeColumn(1.5f); // Payee
                        columns.ConstantColumn(80);  // Amount
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Date").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Description").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Category").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Fund").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Payee").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Amount").FontColor(Colors.White).Bold().AlignRight();
                    });

                    // Rows
                    foreach (var tx in transactions)
                    {
                        var bgColor = tx.Type == Models.TransactionType.Income 
                            ? Colors.Green.Lighten5 : Colors.White;
                        var amountColor = tx.Type == Models.TransactionType.Income 
                            ? Colors.Green.Darken2 : Colors.Red.Darken2;

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(tx.Date.ToString("MM/dd/yyyy"));
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(tx.Description ?? "—");
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(tx.CategoryName ?? "—");
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(tx.FundName ?? "—");
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(tx.Payee ?? tx.DonorName ?? "—");
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text($"{(tx.Type == Models.TransactionType.Income ? "+" : "-")}{tx.Amount:C}")
                            .FontColor(amountColor).AlignRight();
                    }

                    // Totals
                    var totalIncome = transactions.Where(t => t.Type == Models.TransactionType.Income).Sum(t => t.Amount);
                    var totalExpense = transactions.Where(t => t.Type == Models.TransactionType.Expense).Sum(t => t.Amount);
                    
                    table.Cell().ColumnSpan(5).Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Total Income:").Bold().AlignRight();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text($"+{totalIncome:C}").Bold().FontColor(Colors.Green.Darken2).AlignRight();
                    
                    table.Cell().ColumnSpan(5).Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Total Expenses:").Bold().AlignRight();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text($"-{totalExpense:C}").Bold().FontColor(Colors.Red.Darken2).AlignRight();
                    
                    table.Cell().ColumnSpan(5).Background(theme.PrimaryColor).Padding(5)
                        .Text("Net:").Bold().FontColor(Colors.White).AlignRight();
                    table.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text($"{totalIncome - totalExpense:C}").Bold().FontColor(Colors.White).AlignRight();
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateDonorReportAsync()
    {
        var donors = await _donorService.GetAllAsync();
        var theme = GetThemeColors(ReportTheme.Modern);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Text("Donor Summary Report").FontSize(20).Bold().FontColor(theme.PrimaryColor);

                page.Content().PaddingVertical(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Donor Name").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Email").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Type").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Total").FontColor(Colors.White).Bold().AlignRight();
                    });

                    foreach (var donor in donors.OrderByDescending(d => d.TotalContributions))
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(donor.Name);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(donor.Email ?? "—");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(donor.Type.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text($"{donor.TotalContributions:C}").AlignRight();
                    }

                    table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Grand Total:").Bold().AlignRight();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text($"{donors.Sum(d => d.TotalContributions):C}").Bold().AlignRight();
                });

                page.Footer().Element(c => ComposeFooter(c, theme));
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateGrantReportAsync()
    {
        var grants = await _grantService.GetAllAsync();
        var theme = GetThemeColors(ReportTheme.Modern);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(40);

                page.Header().Text("Grant Status Report").FontSize(20).Bold().FontColor(theme.PrimaryColor);

                page.Content().PaddingVertical(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);   // Name
                        columns.RelativeColumn(2);   // Grantor
                        columns.ConstantColumn(70);  // Status
                        columns.ConstantColumn(90);  // Amount
                        columns.ConstantColumn(90);  // Used
                        columns.ConstantColumn(90);  // Remaining
                        columns.ConstantColumn(80);  // End Date
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Grant Name").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Grantor").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Status").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Amount").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Used").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Remaining").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("End Date").FontColor(Colors.White).Bold();
                    });

                    foreach (var grant in grants)
                    {
                        var statusColor = grant.Status switch
                        {
                            Models.GrantStatus.Active => Colors.Green.Lighten5,
                            Models.GrantStatus.Expired => Colors.Red.Lighten5,
                            _ => Colors.White
                        };

                        table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(grant.Name);
                        table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(grant.GrantorName);
                        table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(grant.Status.ToString());
                        table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text($"{grant.Amount:C}").AlignRight();
                        table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text($"{grant.AmountUsed:C}").AlignRight();
                        table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text($"{grant.RemainingBalance:C}").AlignRight();
                        table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(grant.EndDate?.ToString("MM/dd/yyyy") ?? "—");
                    }

                    // Totals
                    table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Totals:").Bold().AlignRight();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text($"{grants.Sum(g => g.Amount):C}").Bold().AlignRight();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text($"{grants.Sum(g => g.AmountUsed):C}").Bold().AlignRight();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text($"{grants.Sum(g => g.RemainingBalance):C}").Bold().AlignRight();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("");
                });

                page.Footer().Element(c => ComposeFooter(c, theme));
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateBudgetReportAsync(DateTime startDate, DateTime endDate)
    {
        var categories = await _categoryService.GetAllAsync();
        var theme = GetThemeColors(ReportTheme.Modern);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Column(col =>
                {
                    col.Item().Text("Budget vs Actual Report").FontSize(20).Bold().FontColor(theme.PrimaryColor);
                    col.Item().Text($"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(60);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Category").FontColor(Colors.White).Bold();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Budget").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Actual").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("Variance").FontColor(Colors.White).Bold().AlignRight();
                        header.Cell().Background(theme.PrimaryColor).Padding(5)
                            .Text("%").FontColor(Colors.White).Bold().AlignRight();
                    });

                    foreach (var cat in categories.Where(c => c.BudgetLimit.HasValue))
                    {
                        var budget = cat.BudgetLimit ?? 0;
                        var actual = 0m; // Would need transaction data
                        var variance = budget - actual;
                        var pct = budget > 0 ? (actual / budget * 100) : 0;

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(cat.Name);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text($"{budget:C}").AlignRight();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text($"{actual:C}").AlignRight();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text($"{variance:C}").FontColor(variance >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text($"{pct:F0}%").AlignRight();
                    }
                });

                page.Footer().Element(c => ComposeFooter(c, theme));
            });
        });

        return document.GeneratePdf();
    }

    // Helper methods
    private void ComposeHeader(IContainer container, string title, ReportRequest request, ThemeColors theme)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(title).FontSize(24).Bold().FontColor(theme.PrimaryColor);
                    col.Item().Text($"Period: {request.StartDate:MMMM dd, yyyy} - {request.EndDate:MMMM dd, yyyy}")
                        .FontSize(11).FontColor(Colors.Grey.Medium);
                });
                
                row.ConstantItem(100).Column(col =>
                {
                    col.Item().Text("NonProfit Finance").FontSize(10).AlignRight();
                    col.Item().Text(DateTime.Now.ToString("MMM dd, yyyy")).FontSize(9).FontColor(Colors.Grey.Medium).AlignRight();
                });
            });
            
            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(theme.PrimaryColor);
        });
    }

    private void ComposeSummarySection(IContainer container, IncomeExpenseSummaryDto summary, ThemeColors theme)
    {
        container.Column(column =>
        {
            column.Item().Text("Financial Summary").FontSize(14).Bold().FontColor(theme.PrimaryColor);
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Background(Colors.Green.Lighten5).Padding(15).Column(col =>
                {
                    col.Item().Text("Total Income").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"{summary.TotalIncome:C}").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                });
                
                row.ConstantItem(10);
                
                row.RelativeItem().Background(Colors.Red.Lighten5).Padding(15).Column(col =>
                {
                    col.Item().Text("Total Expenses").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"{summary.TotalExpenses:C}").FontSize(18).Bold().FontColor(Colors.Red.Darken2);
                });
                
                row.ConstantItem(10);
                
                row.RelativeItem().Background(Colors.Blue.Lighten5).Padding(15).Column(col =>
                {
                    col.Item().Text("Net").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"{summary.NetIncome:C}").FontSize(18).Bold()
                        .FontColor(summary.NetIncome >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                });
            });
        });
    }

    private void ComposeCategorySection(IContainer container, string title, 
        List<CategorySummaryDto> categories, ThemeColors theme, bool isIncome)
    {
        container.Column(column =>
        {
            column.Item().Text(title).FontSize(14).Bold().FontColor(theme.PrimaryColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(60);
                });

                // Add Header Row
                table.Header(header =>
                {
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("Category").FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("Income").FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("Expense").FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("Net").FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("%").FontColor(Colors.White).Bold().AlignRight();
                });

                var totalAmount = categories.Sum(c => c.Amount);
                
                foreach (var cat in categories.Take(10))
                {
                    var net = isIncome ? cat.Amount : -cat.Amount;
                    var netColor = net >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                    
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(cat.CategoryName);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(isIncome ? $"{cat.Amount:C}" : "$0.00").FontColor(Colors.Green.Darken2).AlignRight();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(!isIncome ? $"{cat.Amount:C}" : "$0.00").FontColor(Colors.Red.Darken2).AlignRight();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text($"{net:C}").FontColor(netColor).AlignRight();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text($"{cat.Percentage:F1}%").AlignRight();
                }

                var totalNet = isIncome ? totalAmount : -totalAmount;
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Total:").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                    .Text(isIncome ? $"{totalAmount:C}" : "$0.00").Bold().FontColor(Colors.Green.Darken2).AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                    .Text(!isIncome ? $"{totalAmount:C}" : "$0.00").Bold().FontColor(Colors.Red.Darken2).AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                    .Text($"{totalNet:C}").Bold().FontColor(totalNet >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("100%").Bold().AlignRight();
            });
        });
    }

    private void ComposeTrendSection(IContainer container, List<TrendDataDto> trends, ThemeColors theme)
    {
        container.Column(column =>
        {
            column.Item().Text("Monthly Trend").FontSize(14).Bold().FontColor(theme.PrimaryColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                });

                table.Header(header =>
                {
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("Month").FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("Income").FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("Expenses").FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(5)
                        .Text("Net").FontColor(Colors.White).Bold().AlignRight();
                });

                foreach (var month in trends)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(month.Period.ToString("MMM yyyy"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text($"{month.Income:C}").FontColor(Colors.Green.Darken2).AlignRight();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text($"{month.Expenses:C}").FontColor(Colors.Red.Darken2).AlignRight();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text($"{month.NetIncome:C}").FontColor(month.NetIncome >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
                }
            });
        });
    }

    private void ComposeFooter(IContainer container, ThemeColors theme)
    {
        container.AlignCenter().Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Generated: {DateTime.Now:MMMM dd, yyyy h:mm tt}")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    private ThemeColors GetThemeColors(ReportTheme theme) => theme switch
    {
        ReportTheme.Modern => new ThemeColors("#2563EB", "#1E293B", "#F8FAFC"),
        ReportTheme.Classic => new ThemeColors("#1E40AF", "#111827", "#FFFFFF"),
        ReportTheme.Dark => new ThemeColors("#60A5FA", "#E5E7EB", "#1F2937"),
        ReportTheme.Light => new ThemeColors("#3B82F6", "#374151", "#FFFFFF"),
        ReportTheme.FireDepartment => new ThemeColors("#C41E3A", "#1A1A1A", "#FFFFFF"),
        _ => new ThemeColors("#C41E3A", "#1A1A1A", "#FFFFFF")
    };

    /// <summary>
    /// Composes a verification block for data integrity verification in PDF exports.
    /// </summary>
    private void ComposeVerificationBlock(IContainer container, string reportName, int recordCount, decimal? totalAmount)
    {
        var verification = _dataIntegrityService.CreateVerificationBlock(
            reportName, 
            recordCount, 
            totalAmount, 
            DateTime.UtcNow);
        
        container.Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
        {
            col.Item().Text("VERIFICATION BLOCK").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Records: {verification.RecordCount:N0}").FontSize(7).FontColor(Colors.Grey.Darken1);
                if (verification.TotalAmount.HasValue)
                {
                    row.RelativeItem().Text($"Total: {verification.TotalAmount:C}").FontSize(7).FontColor(Colors.Grey.Darken1);
                }
                row.RelativeItem().Text($"Hash: {verification.VerificationHash}").FontSize(7).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private record ThemeColors(string PrimaryColor, string TextColor, string BackgroundColor);
}
