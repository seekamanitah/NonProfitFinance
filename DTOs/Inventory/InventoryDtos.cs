using NonProfitFinance.Models.Enums;

namespace NonProfitFinance.DTOs.Inventory;

// ==================== INVENTORY ITEM ====================

public record InventoryItemDto(
    int Id,
    string Name,
    string? SKU,
    string? Description,
    int? CategoryId,
    string? CategoryName,
    int? LocationId,
    string? LocationName,
    decimal Quantity,
    string UnitOfMeasure,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal? ReorderPoint,
    decimal UnitCost,
    decimal TotalValue,
    InventoryStatus Status,
    string? ImageUrl,
    string? Barcode,
    string? Manufacturer,
    string? Supplier,
    DateTime? PurchaseDate,
    DateTime? ExpiryDate,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateInventoryItemRequest(
    string Name,
    string? SKU,
    string? Description,
    int? CategoryId,
    int? LocationId,
    decimal Quantity,
    string UnitOfMeasure,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal UnitCost,
    string? ImageUrl,
    string? Barcode,
    string? Manufacturer,
    DateTime? PurchaseDate,
    DateTime? ExpiryDate,
    string? Notes
);

public record UpdateInventoryItemRequest(
    string Name,
    string? SKU,
    string? Description,
    int? CategoryId,
    int? LocationId,
    decimal Quantity,
    string UnitOfMeasure,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal UnitCost,
    string? ImageUrl,
    string? Barcode,
    string? Manufacturer,
    DateTime? PurchaseDate,
    DateTime? ExpiryDate,
    string? Notes,
    bool IsActive
);

public record InventoryItemSummaryDto(
    int Id,
    string Name,
    string? SKU,
    int? CategoryId,
    string? CategoryName,
    decimal Quantity,
    string UnitOfMeasure,
    decimal TotalValue,
    InventoryStatus Status
);

// ==================== INVENTORY CATEGORY ====================

public record InventoryCategoryDto(
    int Id,
    string Name,
    string? Description,
    int? ParentCategoryId,
    string? ParentCategoryName,
    string? ColorCode,
    string? Icon,
    int ItemCount,
    bool IsActive,
    DateTime CreatedAt,
    List<InventoryCategoryDto>? SubCategories
);

public record CreateInventoryCategoryRequest(
    string Name,
    string? Description,
    int? ParentCategoryId,
    string? ColorCode,
    string? Icon
);

public record UpdateInventoryCategoryRequest(
    string Name,
    string? Description,
    int? ParentCategoryId,
    string? ColorCode,
    string? Icon,
    bool IsActive
);

// ==================== LOCATION ====================

public record LocationDto(
    int Id,
    string Name,
    string? Description,
    int? ParentLocationId,
    string? ParentLocationName,
    LocationType Type,
    string? Address,
    string? ContactPerson,
    string? Phone,
    int ItemCount,
    bool IsActive,
    DateTime CreatedAt,
    List<LocationDto>? SubLocations
);

public record CreateLocationRequest(
    string Name,
    string? Description,
    int? ParentLocationId,
    LocationType Type,
    string? Address,
    string? ContactPerson,
    string? Phone
);

public record UpdateLocationRequest(
    string Name,
    string? Description,
    int? ParentLocationId,
    LocationType Type,
    string? Address,
    string? ContactPerson,
    string? Phone,
    bool IsActive
);

// ==================== INVENTORY TRANSACTION ====================

public record InventoryTransactionDto(
    int Id,
    int ItemId,
    string ItemName,
    string? ItemSKU,
    InventoryTransactionType Type,
    decimal Quantity,
    string UnitOfMeasure,
    int? FromLocationId,
    string? FromLocationName,
    int? ToLocationId,
    string? ToLocationName,
    string? Reason,
    decimal? UnitCost,
    decimal? TotalCost,
    string? ReferenceNumber,
    int? RelatedFinancialTransactionId,
    string? PerformedBy,
    string? Notes,
    DateTime TransactionDate,
    DateTime CreatedAt
);

public record CreateInventoryTransactionRequest(
    int ItemId,
    InventoryTransactionType Type,
    decimal Quantity,
    int? FromLocationId,
    int? ToLocationId,
    string? Reason,
    decimal? UnitCost,
    string? ReferenceNumber,
    int? RelatedFinancialTransactionId,
    string? Notes,
    DateTime? TransactionDate
);

public record InventoryTransactionFilterRequest(
    DateTime? StartDate,
    DateTime? EndDate,
    int? ItemId,
    int? LocationId,
    InventoryTransactionType? Type,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 25
);

// ==================== FILTERS AND REQUESTS ====================

public record InventoryItemFilterRequest(
    int? CategoryId,
    int? LocationId,
    InventoryStatus? Status,
    bool? LowStock,
    string? SearchTerm,
    string? SortBy,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 25
);

// ==================== DASHBOARD & REPORTS ====================

public record InventoryDashboardDto(
    int TotalItems,
    int ActiveItems,
    int LowStockItems,
    int OutOfStockItems,
    decimal TotalInventoryValue,
    List<CategoryStockDto> TopCategories,
    List<InventoryItemSummaryDto> LowStockAlerts,
    List<InventoryTransactionDto> RecentTransactions
);

public record CategoryStockDto(
    int CategoryId,
    string CategoryName,
    int ItemCount,
    decimal TotalValue,
    int LowStockCount
);

public record LocationStockDto(
    int LocationId,
    string LocationName,
    int ItemCount,
    decimal TotalValue
);

public record StockLevelReportDto(
    List<InventoryItemDto> Items,
    decimal TotalValue,
    int LowStockCount,
    int OutOfStockCount
);

public record InventoryUsageReportDto(
    DateTime StartDate,
    DateTime EndDate,
    int ItemId,
    string ItemName,
    decimal TotalAdded,
    decimal TotalRemoved,
    decimal NetChange,
    decimal AverageDailyUsage
);

// ==================== PAGED RESULTS ====================

public record PagedInventoryItems(
    List<InventoryItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record PagedInventoryTransactions(
    List<InventoryTransactionDto> Transactions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
