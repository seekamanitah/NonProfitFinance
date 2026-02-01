using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface IPrintableFormsService
{
    /// <summary>
    /// Generate donation receipt
    /// </summary>
    Task<byte[]> GenerateDonationReceiptAsync(int donorId, int transactionId);
    
    /// <summary>
    /// Generate reimbursement request form
    /// </summary>
    byte[] GenerateReimbursementRequestForm(ReimbursementRequestData data);
    
    /// <summary>
    /// Generate check request form
    /// </summary>
    byte[] GenerateCheckRequestForm(CheckRequestData data);
    
    /// <summary>
    /// Generate expense report
    /// </summary>
    byte[] GenerateExpenseReport(ExpenseReportData data);
    
    /// <summary>
    /// Generate blank grant application form
    /// </summary>
    byte[] GenerateGrantApplicationForm(string grantName = "");
}

public class PrintableFormsService : IPrintableFormsService
{
    private readonly IDonorService _donorService;
    private readonly ITransactionService _transactionService;

    public PrintableFormsService(
        IDonorService donorService,
        ITransactionService transactionService)
    {
        _donorService = donorService;
        _transactionService = transactionService;
    }

    public async Task<byte[]> GenerateDonationReceiptAsync(int donorId, int transactionId)
    {
        var donor = await _donorService.GetByIdAsync(donorId);
        var transaction = await _transactionService.GetByIdAsync(transactionId);

        if (donor == null || transaction == null)
            throw new InvalidOperationException("Donor or transaction not found");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Column(col =>
                {
                    col.Item().Text("DONATION RECEIPT").FontSize(24).Bold().AlignCenter();
                    col.Item().Text("Tax-Deductible Contribution").FontSize(12).AlignCenter();
                    col.Item().PaddingTop(10).LineHorizontal(2);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Organization Info
                    col.Item().Text("Issued By:").FontSize(12).Bold();
                    col.Item().Text("NonProfit Organization Name").FontSize(11);
                    col.Item().Text("123 Main Street").FontSize(10);
                    col.Item().Text("City, State 12345").FontSize(10);
                    col.Item().Text("EIN: 12-3456789").FontSize(10);
                    col.Item().PaddingTop(10);

                    // Receipt Info
                    col.Item().Text($"Receipt Number: {transaction.Id:D6}").FontSize(10);
                    col.Item().Text($"Date: {transaction.Date:MMMM dd, yyyy}").FontSize(10);
                    col.Item().PaddingTop(15).LineHorizontal(1);

                    // Donor Info
                    col.Item().PaddingTop(15).Text("Received From:").FontSize(12).Bold();
                    col.Item().Text(donor.Name).FontSize(11);
                    if (!string.IsNullOrEmpty(donor.Address))
                    {
                        col.Item().Text(donor.Address).FontSize(10);
                    }
                    col.Item().PaddingTop(15);

                    // Donation Details
                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(innerCol =>
                    {
                        innerCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Donation Amount:").FontSize(12).Bold();
                            row.RelativeItem().Text(transaction.Amount.ToString("C")).FontSize(14).Bold().AlignRight();
                        });
                        
                        if (!string.IsNullOrEmpty(transaction.Description))
                        {
                            innerCol.Item().PaddingTop(5).Text($"Purpose: {transaction.Description}").FontSize(10);
                        }
                    });

                    col.Item().PaddingTop(20);

                    // Tax Statement
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(innerCol =>
                    {
                        innerCol.Item().Text("Tax-Deductible Information").FontSize(11).Bold();
                        innerCol.Item().PaddingTop(5).Text(
                            "No goods or services were provided in exchange for this contribution. " +
                            "This receipt serves as official documentation for tax purposes. " +
                            "Please retain for your records."
                        ).FontSize(9).LineHeight(1.3f);
                    });

                    col.Item().PaddingTop(30);
                    col.Item().Text("Thank you for your generous support!").FontSize(12).Bold().AlignCenter();
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated: ");
                    text.Span(DateTime.Now.ToString("MMMM dd, yyyy h:mm tt")).FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateReimbursementRequestForm(ReimbursementRequestData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Column(col =>
                {
                    col.Item().Text("REIMBURSEMENT REQUEST FORM").FontSize(20).Bold().AlignCenter();
                    col.Item().PaddingTop(5).LineHorizontal(2);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Requestor Information
                    col.Item().Text("Requestor Information").FontSize(14).Bold();
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        AddFormField(table, "Name:", data.RequestorName);
                        AddFormField(table, "Date:", data.RequestDate.ToString("MM/dd/yyyy"));
                        AddFormField(table, "Department:", data.Department);
                        AddFormField(table, "Phone:", data.Phone);
                        AddFormField(table, "Email:", data.Email);
                    });

                    col.Item().PaddingTop(20).Text("Expense Details").FontSize(14).Bold();
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.ConstantColumn(100);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Date").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Description").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Category").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Amount").Bold().AlignRight();
                        });

                        foreach (var item in data.Expenses)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(item.Date.ToString("MM/dd/yyyy")).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(item.Description).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(item.Category).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(item.Amount.ToString("C")).FontSize(9).AlignRight();
                        }

                        table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten4).Padding(5)
                            .Text("Total Reimbursement:").Bold().AlignRight();
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                            .Text(data.Expenses.Sum(e => e.Amount).ToString("C")).Bold().AlignRight();
                    });

                    col.Item().PaddingTop(20).Text("Additional Information").FontSize(14).Bold();
                    col.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Medium).Padding(10).MinHeight(60)
                        .Text(data.Notes ?? "(none)").FontSize(10);

                    col.Item().PaddingTop(30).Text("Approvals").FontSize(14).Bold();
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(approvalCol =>
                        {
                            approvalCol.Item().Text("Supervisor Signature:").FontSize(10).Bold();
                            approvalCol.Item().PaddingTop(20).LineHorizontal(1);
                            approvalCol.Item().PaddingTop(5).Text("Date:").FontSize(9);
                            approvalCol.Item().LineHorizontal(1);
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Column(approvalCol =>
                        {
                            approvalCol.Item().Text("Finance Approval:").FontSize(10).Bold();
                            approvalCol.Item().PaddingTop(20).LineHorizontal(1);
                            approvalCol.Item().PaddingTop(5).Text("Date:").FontSize(9);
                            approvalCol.Item().LineHorizontal(1);
                        });
                    });
                });

                page.Footer().AlignCenter().Text($"Form #RR-{data.RequestId} | Generated: {DateTime.Now:MM/dd/yyyy}").FontSize(8);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateCheckRequestForm(CheckRequestData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Column(col =>
                {
                    col.Item().Text("CHECK REQUEST FORM").FontSize(20).Bold().AlignCenter();
                    col.Item().PaddingTop(5).LineHorizontal(2);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Request #: {data.RequestId}").FontSize(10);
                        row.RelativeItem().Text($"Date: {data.RequestDate:MM/dd/yyyy}").FontSize(10).AlignRight();
                    });

                    col.Item().PaddingTop(20).Text("Payment Information").FontSize(14).Bold();
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        AddFormField(table, "Payable To:", data.PayeeName);
                        AddFormField(table, "Amount:", data.Amount.ToString("C"));
                        AddFormField(table, "Account:", data.AccountName);
                        AddFormField(table, "Category:", data.Category);
                    });

                    col.Item().PaddingTop(15).Text("Payee Address:").FontSize(12).Bold();
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).MinHeight(60)
                        .Text(data.PayeeAddress ?? "(not provided)").FontSize(10);

                    col.Item().PaddingTop(15).Text("Purpose/Description:").FontSize(12).Bold();
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).MinHeight(60)
                        .Text(data.Purpose).FontSize(10);

                    col.Item().PaddingTop(20).Text("Requested By:").FontSize(12).Bold();
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        AddFormField(table, "Name:", data.RequestorName);
                        AddFormField(table, "Department:", data.Department);
                    });

                    col.Item().PaddingTop(30).Text("Approvals").FontSize(14).Bold();
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(approvalCol =>
                        {
                            approvalCol.Item().Text("Authorized Signature:").FontSize(10).Bold();
                            approvalCol.Item().PaddingTop(20).LineHorizontal(1);
                            approvalCol.Item().PaddingTop(5).Text("Date:").FontSize(9);
                            approvalCol.Item().LineHorizontal(1);
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Column(approvalCol =>
                        {
                            approvalCol.Item().Text("Finance Approval:").FontSize(10).Bold();
                            approvalCol.Item().PaddingTop(20).LineHorizontal(1);
                            approvalCol.Item().PaddingTop(5).Text("Date:").FontSize(9);
                            approvalCol.Item().LineHorizontal(1);
                        });
                    });
                });

                page.Footer().AlignCenter().Text($"Form #CR-{data.RequestId} | Generated: {DateTime.Now:MM/dd/yyyy}").FontSize(8);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateExpenseReport(ExpenseReportData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(40);

                page.Header().Column(col =>
                {
                    col.Item().Text("EXPENSE REPORT").FontSize(20).Bold().AlignCenter();
                    col.Item().Text($"{data.StartDate:MMM dd, yyyy} - {data.EndDate:MMM dd, yyyy}").FontSize(12).AlignCenter();
                    col.Item().PaddingTop(5).LineHorizontal(2);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Report #: {data.ReportId}").FontSize(10);
                        row.RelativeItem().Text($"Submitted By: {data.SubmittedBy}").FontSize(10).AlignRight();
                    });

                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1);
                            columns.ConstantColumn(90);
                            columns.ConstantColumn(90);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Date").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Description").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Vendor").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Category").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Account").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Amount").FontColor(Colors.White).Bold().AlignRight();
                        });

                        foreach (var expense in data.Expenses)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(expense.Date.ToString("MM/dd")).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(expense.Description).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(expense.Vendor ?? "-").FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(expense.Category).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(expense.Account ?? "-").FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(expense.Amount.ToString("C")).FontSize(9).AlignRight();
                        }

                        // Totals
                        table.Cell().ColumnSpan(5).Background(Colors.Grey.Lighten4).Padding(5)
                            .Text("Total Expenses:").Bold().AlignRight();
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(5)
                            .Text(data.Expenses.Sum(e => e.Amount).ToString("C")).Bold().AlignRight();
                    });

                    col.Item().PaddingTop(15).Text("Summary by Category").FontSize(12).Bold();
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(100);
                            columns.ConstantColumn(80);
                        });

                        var categoryTotals = data.Expenses.GroupBy(e => e.Category)
                            .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount), Count = g.Count() })
                            .OrderByDescending(x => x.Amount);

                        foreach (var cat in categoryTotals)
                        {
                            table.Cell().Padding(4).Text(cat.Category).FontSize(10);
                            table.Cell().Padding(4).Text(cat.Amount.ToString("C")).FontSize(10).AlignRight();
                            table.Cell().Padding(4).Text($"{cat.Count} items").FontSize(9).AlignRight();
                        }
                    });
                });

                page.Footer().AlignCenter().Text($"Page ").FontSize(8);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateGrantApplicationForm(string grantName = "")
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Column(col =>
                {
                    col.Item().Text("GRANT APPLICATION FORM").FontSize(20).Bold().AlignCenter();
                    if (!string.IsNullOrEmpty(grantName))
                    {
                        col.Item().Text(grantName).FontSize(12).AlignCenter();
                    }
                    col.Item().PaddingTop(5).LineHorizontal(2);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Text("Applicant Information").FontSize(14).Bold();
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        AddBlankFormField(table, "Organization Name:");
                        AddBlankFormField(table, "EIN/Tax ID:");
                        AddBlankFormField(table, "Contact Person:");
                        AddBlankFormField(table, "Title:");
                        AddBlankFormField(table, "Phone:");
                        AddBlankFormField(table, "Email:");
                    });

                    col.Item().PaddingTop(15).Text("Organization Address:").FontSize(12).Bold();
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).MinHeight(60).Text("");

                    col.Item().PaddingTop(15).Text("Grant Request Details").FontSize(14).Bold();
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        AddBlankFormField(table, "Amount Requested:");
                        AddBlankFormField(table, "Project Duration:");
                        AddBlankFormField(table, "Start Date:");
                        AddBlankFormField(table, "End Date:");
                    });

                    col.Item().PaddingTop(15).Text("Project Description:").FontSize(12).Bold();
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).MinHeight(100).Text("");

                    col.Item().PaddingTop(15).Text("Project Goals and Objectives:").FontSize(12).Bold();
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).MinHeight(100).Text("");

                    col.Item().PaddingTop(15).Text("Budget Breakdown:").FontSize(12).Bold();
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).MinHeight(100).Text("");

                    col.Item().PaddingTop(20).Text("Certification").FontSize(12).Bold();
                    col.Item().PaddingTop(5).Text("I certify that the information provided is accurate and complete.").FontSize(9);
                    col.Item().PaddingTop(15).Row(row =>
                    {
                        row.RelativeItem().Column(signCol =>
                        {
                            signCol.Item().Text("Signature:").FontSize(10);
                            signCol.Item().PaddingTop(15).LineHorizontal(1);
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Column(dateCol =>
                        {
                            dateCol.Item().Text("Date:").FontSize(10);
                            dateCol.Item().PaddingTop(15).LineHorizontal(1);
                        });
                    });
                });

                page.Footer().AlignCenter().Text($"Generated: {DateTime.Now:MM/dd/yyyy}").FontSize(8);
            });
        });

        return document.GeneratePdf();
    }

    private void AddFormField(TableDescriptor table, string label, string value)
    {
        table.Cell().Padding(4).Text(label).FontSize(10).Bold();
        table.Cell().Padding(4).Text(value ?? "").FontSize(10);
    }

    private void AddBlankFormField(TableDescriptor table, string label)
    {
        table.Cell().Padding(4).Text(label).FontSize(10).Bold();
        table.Cell().Padding(4).LineHorizontal(1);
    }
}

// Data Transfer Objects for Forms
public record ReimbursementRequestData(
    string RequestId,
    DateTime RequestDate,
    string RequestorName,
    string Department,
    string Phone,
    string Email,
    List<ExpenseLineItem> Expenses,
    string? Notes
);

public record ExpenseLineItem(
    DateTime Date,
    string Description,
    string Category,
    decimal Amount,
    string? Vendor = null,
    string? Account = null
);

public record CheckRequestData(
    string RequestId,
    DateTime RequestDate,
    string PayeeName,
    string? PayeeAddress,
    decimal Amount,
    string Purpose,
    string AccountName,
    string Category,
    string RequestorName,
    string Department
);

public record ExpenseReportData(
    string ReportId,
    DateTime StartDate,
    DateTime EndDate,
    string SubmittedBy,
    List<ExpenseLineItem> Expenses
);
