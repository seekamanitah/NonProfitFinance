using System.ComponentModel.DataAnnotations;
using NonProfitFinance.Models;

namespace NonProfitFinance.DTOs;

// ============ Category DTOs ============

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    string? Color,
    string? Icon,
    CategoryType Type,
    decimal? BudgetLimit,
    bool IsArchived,
    int SortOrder,
    int? ParentId,
    string? ParentName,
    List<CategoryDto>? Children = null
);

public record CreateCategoryRequest(
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    string Name,
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    string? Description,
    [StringLength(7, ErrorMessage = "Color must be a valid hex code")]
    string? Color,
    [StringLength(50, ErrorMessage = "Icon cannot exceed 50 characters")]
    string? Icon,
    CategoryType Type,
    [Range(0, 10000000, ErrorMessage = "Budget limit must be between 0 and 10,000,000")]
    decimal? BudgetLimit,
    int? ParentId,
    int SortOrder = 0
);

public record UpdateCategoryRequest(
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    string Name,
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    string? Description,
    [StringLength(7, ErrorMessage = "Color must be a valid hex code")]
    string? Color,
    [StringLength(50, ErrorMessage = "Icon cannot exceed 50 characters")]
    string? Icon,
    [Range(0, 10000000, ErrorMessage = "Budget limit must be between 0 and 10,000,000")]
    decimal? BudgetLimit,
    int? ParentId,
    int SortOrder
);

// ============ Transaction DTOs ============

public record TransactionDto(
    int Id,
    DateTime Date,
    decimal Amount,
    string? Description,
    TransactionType Type,
    int CategoryId,
    string? CategoryName,
    FundType FundType,
    int? FundId,
    string? FundName,
    int? ToFundId,
    string? ToFundName,
    Guid? TransferPairId,
    int? DonorId,
    string? DonorName,
    int? GrantId,
    string? GrantName,
    string? Payee,
    string? Tags,
    string? ReferenceNumber,
    string? PONumber,
    bool IsRecurring,
    bool IsReconciled,
    List<TransactionSplitDto>? Splits = null
);

public record CreateTransactionRequest(
[Required(ErrorMessage = "Date is required")]
DateTime Date,
[Required(ErrorMessage = "Amount is required")]
[Range(0.01, 100000000, ErrorMessage = "Amount must be between $0.01 and $100,000,000")]
decimal Amount,
[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
string? Description,
TransactionType Type,
[Required(ErrorMessage = "Category is required")]
int CategoryId,
FundType FundType = FundType.Unrestricted,
int? FundId = null,
int? ToFundId = null,
int? DonorId = null,
int? GrantId = null,
[StringLength(200, ErrorMessage = "Payee cannot exceed 200 characters")]
string? Payee = null,
[StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
string? Tags = null,
[StringLength(50, ErrorMessage = "Reference number cannot exceed 50 characters")]
string? ReferenceNumber = null,
[StringLength(50, ErrorMessage = "PO number cannot exceed 50 characters")]
string? PONumber = null,
bool IsRecurring = false,
    string? RecurrencePattern = null,
    List<CreateTransactionSplitRequest>? Splits = null
);

public record UpdateTransactionRequest(
    DateTime Date,
    decimal Amount,
    string? Description,
    TransactionType Type,
    int CategoryId,
    FundType FundType,
    int? FundId,
    int? ToFundId,
    int? DonorId,
    int? GrantId,
    string? Payee,
    string? Tags,
    string? ReferenceNumber,
    string? PONumber,
    bool IsReconciled,
    List<CreateTransactionSplitRequest>? Splits = null
);

public record TransactionSplitDto(
    int Id,
    int CategoryId,
    string? CategoryName,
    decimal Amount,
    string? Description
);

public record CreateTransactionSplitRequest(
    int CategoryId,
    decimal Amount,
    string? Description
);

public record TransactionFilterRequest(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? CategoryId = null,
    int? FundId = null,
    int? DonorId = null,
    int? GrantId = null,
    TransactionType? Type = null,
    string? SearchTerm = null,
    string? Tags = null,
    bool IncludeArchived = false,
    int Page = 1,
    int PageSize = 50
);

// ============ Donor DTOs ============

public record DonorDto(
    int Id,
    string Name,
    DonorType Type,
    string? Email,
    string? Phone,
    string? Address,
    string? Notes,
    decimal TotalContributions,
    DateTime? FirstContributionDate,
    DateTime? LastContributionDate,
    bool IsAnonymous,
    bool IsActive
);

public record CreateDonorRequest(
    string Name,
    DonorType Type = DonorType.Individual,
    string? Email = null,
    string? Phone = null,
    string? Address = null,
    string? Notes = null,
    bool IsAnonymous = false
);

public record UpdateDonorRequest(
    string Name,
    DonorType Type,
    string? Email,
    string? Phone,
    string? Address,
    string? Notes,
    bool IsAnonymous,
    bool IsActive
);

// ============ Grant DTOs ============

public record GrantDto(
    int Id,
    string Name,
    string GrantorName,
    decimal Amount,
    decimal AmountUsed,
    decimal RemainingBalance,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? ApplicationDate,
    GrantStatus Status,
    string? Restrictions,
    string? Notes,
    string? GrantNumber,
    string? ContactPerson,
    string? ContactEmail,
    string? ReportingRequirements,
    DateTime? NextReportDueDate
);

public record CreateGrantRequest(
    string Name,
    string GrantorName,
    decimal Amount,
    DateTime StartDate,
    DateTime? EndDate = null,
    DateTime? ApplicationDate = null,
    GrantStatus Status = GrantStatus.Pending,
    string? Restrictions = null,
    string? Notes = null,
    string? GrantNumber = null,
    string? ContactPerson = null,
    string? ContactEmail = null,
    string? ReportingRequirements = null,
    DateTime? NextReportDueDate = null
);

public record UpdateGrantRequest(
    string Name,
    string GrantorName,
    decimal Amount,
    DateTime StartDate,
    DateTime? EndDate,
    GrantStatus Status,
    string? Restrictions,
    string? Notes,
    string? GrantNumber,
    string? ContactPerson,
    string? ContactEmail,
    string? ReportingRequirements,
    DateTime? NextReportDueDate
);

// ============ Fund DTOs ============

public record FundDto(
    int Id,
    string Name,
    FundType Type,
    decimal StartingBalance,
    decimal Balance,
    string? Description,
    decimal? TargetBalance,
    bool IsActive,
    DateTime? RestrictionExpiryDate
);

public record CreateFundRequest(
    string Name,
    FundType Type,
    string? Description = null,
    decimal StartingBalance = 0,
    decimal? TargetBalance = null,
    DateTime? RestrictionExpiryDate = null
);

public record UpdateFundRequest(
    string Name,
    FundType Type,
    string? Description,
    decimal StartingBalance, // Now editable - triggers recalculation of current balance
    decimal? TargetBalance,
    bool IsActive,
    DateTime? RestrictionExpiryDate
);

// ============ Report DTOs ============

public record DashboardMetricsDto(
    decimal MonthlyNetIncome,
    decimal YtdNetIncome,
    decimal TotalCashBalance,
    decimal RestrictedFundsPercentage,
    decimal UnrestrictedBalance,
    decimal RestrictedBalance,
    int ActiveGrantsCount,
    int ActiveDonorsCount,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal YtdRevenue
);

public record IncomeExpenseSummaryDto(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetIncome,
    List<CategorySummaryDto> IncomeByCategory,
    List<CategorySummaryDto> ExpensesByCategory
);

public record CategorySummaryDto(
    int CategoryId,
    string CategoryName,
    string? Color,
    decimal Amount,
    decimal Percentage,
    List<CategorySummaryDto>? Subcategories = null
);

public record TrendDataDto(
    DateTime Period,
    decimal Income,
    decimal Expenses,
    decimal NetIncome
);

public record ReportFilterRequest(
DateTime StartDate,
DateTime EndDate,
int? CategoryId = null,
int? FundId = null,
int? DonorId = null,
int? GrantId = null,
bool IncludeSubcategories = true
);

// ============ Pagination ============

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// ============ Common Response ============

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message = null,
    List<string>? Errors = null
);
