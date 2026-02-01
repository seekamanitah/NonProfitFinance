using NonProfitFinance.Models;

namespace NonProfitFinance.DTOs;

// ============ Budget DTOs ============

public record BudgetDto(
    int Id,
    string Name,
    int FiscalYear,
    decimal TotalBudget,
    decimal TotalSpent,
    decimal Remaining,
    decimal PercentageUsed,
    List<BudgetLineItemDto> LineItems,
    bool IsActive
);

public record BudgetLineItemDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string? CategoryColor,
    decimal BudgetAmount,
    decimal ActualAmount,
    decimal Variance,
    decimal PercentageUsed
);

public record CreateBudgetRequest(
    string Name,
    int FiscalYear,
    List<CreateBudgetLineItemRequest> LineItems
);

public record CreateBudgetLineItemRequest(
    int CategoryId,
    decimal BudgetAmount
);

public record UpdateBudgetRequest(
    string Name,
    bool IsActive,
    List<CreateBudgetLineItemRequest> LineItems
);
