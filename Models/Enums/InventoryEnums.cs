namespace NonProfitFinance.Models.Enums;

/// <summary>
/// Inventory module enumerations
/// </summary>

/// <summary>
/// Status of inventory item
/// </summary>
public enum InventoryStatus
{
    InStock,
    LowStock,
    OutOfStock,
    Discontinued,
    OnOrder
}

/// <summary>
/// Type of inventory transaction
/// </summary>
public enum InventoryTransactionType
{
    Purchase,
    Use,
    Transfer,
    Adjustment,
    Return,
    Disposal,
    Donation
}

/// <summary>
/// Unit of measure for inventory items
/// </summary>
public enum UnitOfMeasure
{
    Each,
    Box,
    Case,
    Pair,
    Set,
    Gallon,
    Liter,
    Pound,
    Kilogram,
    Foot,
    Meter,
    SquareFoot,
    SquareMeter
}

/// <summary>
/// Condition of inventory item
/// </summary>
public enum ItemCondition
{
    New,
    Good,
    Fair,
    Poor,
    Damaged,
    NeedsRepair
}
