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
    private readonly IFundService _fundService;
    private readonly IBudgetService _budgetService;
    private readonly IDataIntegrityService _dataIntegrityService;

    public PdfExportService(
        IReportService reportService,
        ITransactionService transactionService,
        IDonorService donorService,
        IGrantService grantService,
        ICategoryService categoryService,
        IFundService fundService,
        IBudgetService budgetService,
        IDataIntegrityService dataIntegrityService)
    {
        _reportService = reportService;
        _transactionService = transactionService;
        _donorService = donorService;
        _grantService = grantService;
        _categoryService = categoryService;
        _fundService = fundService;
        _budgetService = budgetService;
        _dataIntegrityService = dataIntegrityService;
        
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }


    public async Task<byte[]> GenerateFinancialReportAsync(ReportRequest request)
    {
        return await GenerateFinancialReportAsync(request, new PdfReportOptions());
    }

    public async Task<byte[]> GenerateFinancialReportAsync(ReportRequest request, PdfReportOptions options)
    {
        var filter = new ReportFilterRequest(request.StartDate, request.EndDate,
            request.CategoryId, request.FundId, request.DonorId, request.GrantId);
        var summary = await _reportService.GetIncomeExpenseSummaryAsync(filter);
        var trends = request.IncludeDetails && options.IncludeTrend
            ? await _reportService.GetTrendDataAsync(request.StartDate, request.EndDate)
            : new List<TrendDataDto>();

        var theme = GetThemeColors(request.Theme);
        var title = options.CustomTitle ?? "Financial Report";
        var pageSize = options.LandscapeOrientation ? PageSizes.Letter.Landscape() : PageSizes.Letter;

        // Pre-fetch optional section data
        List<TransactionDto>? transactions = null;
        if (options.IncludeTransactions)
        {
            var txFilter = new TransactionFilterRequest(request.StartDate, request.EndDate,
                request.CategoryId, request.FundId, request.DonorId, request.GrantId);
            var txResult = await _transactionService.GetAllAsync(txFilter);
            transactions = txResult.Items;
        }

        List<DonorDto>? donors = options.IncludeDonors ? await _donorService.GetAllAsync() : null;
        List<GrantDto>? grants = options.IncludeGrants ? await _grantService.GetAllAsync(null) : null;
        List<FundDto>? funds = options.IncludeFunds ? await _fundService.GetAllAsync() : null;
        BudgetDto? budget = null;
        if (options.IncludeBudget)
        {
            try { budget = await _budgetService.GetBudgetVsActualAsync(request.StartDate.Year); }
            catch { /* No budget for this year */ }
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(pageSize);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(theme.TextColor));

                page.Header().Element(c => ComposeHeader(c, title, request, theme));

                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    if (options.IncludeSummary)
                    {
                        column.Item().Element(c => ComposeSummarySection(c, summary, theme));
                    }

                    if (options.IncludeIncomeBreakdown)
                    {
                        column.Item().Element(c => ComposeCategorySection(c, "Income by Category",
                            summary.IncomeByCategory, theme, isIncome: true));
                    }

                    if (options.IncludeExpenseBreakdown)
                    {
                        column.Item().Element(c => ComposeCategorySection(c, "Expenses by Category",
                            summary.ExpensesByCategory, theme, isIncome: false));
                    }

                    if (options.IncludeTrend && trends.Count > 0)
                    {
                        column.Item().Element(c => ComposeTrendSection(c, trends, theme));
                    }

                    if (options.IncludeTransactions && transactions != null)
                    {
                        column.Item().Element(c => ComposeTransactionsSection(c, transactions, request, theme));
                    }

                    if (options.IncludeDonors && donors != null)
                    {
                        column.Item().Element(c => ComposeDonorSection(c, donors, theme));
                    }

                    if (options.IncludeGrants && grants != null)
                    {
                        column.Item().Element(c => ComposeGrantSection(c, grants, theme));
                    }

                    if (options.IncludeFunds && funds != null)
                    {
                        column.Item().Element(c => ComposeFundSection(c, funds, theme));
                    }

                    if (options.IncludeBudget && budget != null)
                    {
                        column.Item().Element(c => ComposeBudgetSection(c, budget, theme));
                    }
                });

                page.Footer().Element(c => ComposeFooter(c, theme));
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateTransactionReportAsync(TransactionFilterRequest filter, ReportTheme reportTheme = ReportTheme.Modern)
    {
        var result = await _transactionService.GetAllAsync(filter);
        var transactions = result.Items;
        var theme = GetThemeColors(reportTheme);

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

    public async Task<byte[]> GenerateDonorReportAsync(ReportTheme reportTheme = ReportTheme.Modern)
    {
        var donors = await _donorService.GetAllAsync();
        var theme = GetThemeColors(reportTheme);

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

    public async Task<byte[]> GenerateGrantReportAsync(ReportTheme reportTheme = ReportTheme.Modern)
    {
        var grants = await _grantService.GetAllAsync(null);
        var theme = GetThemeColors(reportTheme);

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

    public async Task<byte[]> GenerateBudgetReportAsync(DateTime startDate, DateTime endDate, ReportTheme reportTheme = ReportTheme.Modern)
    {
        var theme = GetThemeColors(reportTheme);
        BudgetDto? budget = null;
        try { budget = await _budgetService.GetBudgetVsActualAsync(startDate.Year); }
        catch { /* No budget for this year */ }

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

                    if (budget != null)
                    {
                        foreach (var line in budget.LineItems)
                        {
                            var variance = line.BudgetAmount - line.ActualAmount;
                            var pct = line.PercentageUsed;

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(line.CategoryName);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text($"{line.BudgetAmount:C}").AlignRight();
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text($"{line.ActualAmount:C}").AlignRight();
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text($"{variance:C}").FontColor(variance >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text($"{pct:F0}%").AlignRight();
                        }

                        // Totals row
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Total:").Bold();
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .Text($"{budget.TotalBudget:C}").Bold().AlignRight();
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .Text($"{budget.TotalSpent:C}").Bold().AlignRight();
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .Text($"{budget.Remaining:C}").Bold()
                            .FontColor(budget.Remaining >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .Text($"{budget.PercentageUsed:F0}%").Bold().AlignRight();
                    }
                    else
                    {
                        table.Cell().ColumnSpan(5).Padding(20).AlignCenter()
                            .Text("No budget data available for this period.").FontColor(Colors.Grey.Medium);
                    }
                });

                page.Footer().Element(c => ComposeFooter(c, theme));
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateFundSummaryReportAsync(ReportTheme reportTheme = ReportTheme.Modern)
    {
        var funds = await _fundService.GetAllAsync();
        var theme = GetThemeColors(reportTheme);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Text("Fund / Account Summary").FontSize(20).Bold().FontColor(theme.PrimaryColor);

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Element(c => ComposeFundSection(c, funds, theme));
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

    private void ComposeTransactionsSection(IContainer container, List<TransactionDto> transactions, ReportRequest request, ThemeColors theme)
    {
        container.Column(column =>
        {
            column.Item().Text("Transaction Detail").FontSize(14).Bold().FontColor(theme.PrimaryColor);
            column.Item().Text($"{transactions.Count} transactions from {request.StartDate:MMM d, yyyy} to {request.EndDate:MMM d, yyyy}")
                .FontSize(9).FontColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);  // Date
                    columns.RelativeColumn(3);   // Description
                    columns.RelativeColumn(1.5f); // Category
                    columns.RelativeColumn(1.5f); // Fund
                    columns.ConstantColumn(75);  // Amount
                });

                table.Header(header =>
                {
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Date").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Description").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Category").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Account").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Amount").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                });

                foreach (var tx in transactions)
                {
                    var bgColor = tx.Type == Models.TransactionType.Income
                        ? Colors.Green.Lighten5 : Colors.White;
                    var amountColor = tx.Type == Models.TransactionType.Income
                        ? Colors.Green.Darken2 : Colors.Red.Darken2;
                    var sign = tx.Type == Models.TransactionType.Income ? "+" : "-";

                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(tx.Date.ToString("MM/dd/yy")).FontSize(8);
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(tx.Description ?? "—").FontSize(8);
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(tx.CategoryName ?? "—").FontSize(8);
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(tx.FundName ?? "—").FontSize(8);
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{sign}{tx.Amount:C}").FontSize(8).FontColor(amountColor).AlignRight();
                }

                // Totals
                var totalIncome = transactions.Where(t => t.Type == Models.TransactionType.Income).Sum(t => t.Amount);
                var totalExpense = transactions.Where(t => t.Type == Models.TransactionType.Expense).Sum(t => t.Amount);

                table.Cell().ColumnSpan(4).Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"Income: {totalIncome:C}  |  Expenses: {totalExpense:C}  |  Net: {totalIncome - totalExpense:C}")
                    .FontSize(8).Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{totalIncome - totalExpense:C}").FontSize(8).Bold()
                    .FontColor(totalIncome - totalExpense >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
            });
        });
    }

    private void ComposeDonorSection(IContainer container, List<DonorDto> donors, ThemeColors theme)
    {
        container.Column(column =>
        {
            column.Item().Text("Donor Summary").FontSize(14).Bold().FontColor(theme.PrimaryColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1.5f);
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Donor").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Email").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Type").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Total").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                });

                foreach (var donor in donors.OrderByDescending(d => d.TotalContributions))
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(donor.Name).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(donor.Email ?? "—").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(donor.Type.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text($"{donor.TotalContributions:C}").FontSize(8).AlignRight();
                }

                table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten3).Padding(4)
                    .Text("Grand Total:").FontSize(8).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{donors.Sum(d => d.TotalContributions):C}").FontSize(8).Bold().AlignRight();
            });
        });
    }

    private void ComposeGrantSection(IContainer container, List<GrantDto> grants, ThemeColors theme)
    {
        container.Column(column =>
        {
            column.Item().Text("Grant Status").FontSize(14).Bold().FontColor(theme.PrimaryColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);   // Name
                    columns.RelativeColumn(1.5f); // Grantor
                    columns.ConstantColumn(60);  // Status
                    columns.ConstantColumn(80);  // Amount
                    columns.ConstantColumn(80);  // Remaining
                });

                table.Header(header =>
                {
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Grant").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Grantor").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Status").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Amount").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Remaining").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
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
                        .Padding(3).Text(grant.Name).FontSize(8);
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(grant.GrantorName).FontSize(8);
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(grant.Status.ToString()).FontSize(8);
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{grant.Amount:C}").FontSize(8).AlignRight();
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{grant.RemainingBalance:C}").FontSize(8).AlignRight();
                }

                table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten3).Padding(4)
                    .Text("Totals:").FontSize(8).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{grants.Sum(g => g.Amount):C}").FontSize(8).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{grants.Sum(g => g.RemainingBalance):C}").FontSize(8).Bold().AlignRight();
            });
        });
    }

    private void ComposeFundSection(IContainer container, List<FundDto> funds, ThemeColors theme)
    {
        container.Column(column =>
        {
            column.Item().Text("Fund / Account Summary").FontSize(14).Bold().FontColor(theme.PrimaryColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);   // Name
                    columns.ConstantColumn(80);  // Type
                    columns.ConstantColumn(90);  // Starting Balance
                    columns.ConstantColumn(90);  // Current Balance
                    columns.ConstantColumn(70);  // Status
                });

                table.Header(header =>
                {
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Account").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Type").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Starting").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Current").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Status").FontSize(8).FontColor(Colors.White).Bold();
                });

                foreach (var fund in funds)
                {
                    var statusColor = fund.IsActive ? Colors.White : Colors.Grey.Lighten4;
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(fund.Name).FontSize(8);
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(fund.Type.ToString()).FontSize(8);
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{fund.StartingBalance:C}").FontSize(8).AlignRight();
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{fund.Balance:C}").FontSize(8)
                        .FontColor(fund.Balance >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
                    table.Cell().Background(statusColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(fund.IsActive ? "Active" : "Inactive").FontSize(8);
                }

                // Totals
                table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten3).Padding(4)
                    .Text("Totals:").FontSize(8).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{funds.Sum(f => f.StartingBalance):C}").FontSize(8).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{funds.Sum(f => f.Balance):C}").FontSize(8).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("").FontSize(8);
            });
        });
    }

    private void ComposeBudgetSection(IContainer container, BudgetDto budget, ThemeColors theme)
    {
        container.Column(column =>
        {
            column.Item().Text($"Budget vs Actual — {budget.Name}").FontSize(14).Bold().FontColor(theme.PrimaryColor);
            column.Item().Text($"Fiscal Year {budget.FiscalYear}  |  {budget.PercentageUsed:F0}% used  |  {budget.Remaining:C} remaining")
                .FontSize(9).FontColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(55);
                });

                table.Header(header =>
                {
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Category").FontSize(8).FontColor(Colors.White).Bold();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Budget").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Actual").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("Variance").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                    header.Cell().Background(theme.PrimaryColor).Padding(4)
                        .Text("%").FontSize(8).FontColor(Colors.White).Bold().AlignRight();
                });

                foreach (var line in budget.LineItems)
                {
                    var variance = line.BudgetAmount - line.ActualAmount;
                    var overBudget = line.PercentageUsed > 100;
                    var bgColor = overBudget ? Colors.Red.Lighten5 : Colors.White;

                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text(line.CategoryName).FontSize(8);
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{line.BudgetAmount:C}").FontSize(8).AlignRight();
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{line.ActualAmount:C}").FontSize(8).AlignRight();
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{variance:C}").FontSize(8)
                        .FontColor(variance >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(3).Text($"{line.PercentageUsed:F0}%").FontSize(8).AlignRight();
                }

                // Totals
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Total:").FontSize(8).Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{budget.TotalBudget:C}").FontSize(8).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{budget.TotalSpent:C}").FontSize(8).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{budget.Remaining:C}").FontSize(8).Bold()
                    .FontColor(budget.Remaining >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).AlignRight();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                    .Text($"{budget.PercentageUsed:F0}%").FontSize(8).Bold().AlignRight();
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
