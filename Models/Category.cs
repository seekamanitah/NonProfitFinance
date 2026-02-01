namespace NonProfitFinance.Models;

/// <summary>
/// Represents a hierarchical category for income or expense classification.
/// Supports unlimited nesting levels via self-referencing parent-child relationship.
/// </summary>
public class Category
{
    public int Id { get; set; }

    /// <summary>
    /// Category name (must be unique within the same parent level).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description explaining the category purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Hex color code for UI display (e.g., #FF5733).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Icon identifier for UI display.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Whether this is an income or expense category.
    /// </summary>
    public CategoryType Type { get; set; }

    /// <summary>
    /// Optional budget limit for expense categories.
    /// </summary>
    public decimal? BudgetLimit { get; set; }

    /// <summary>
    /// Soft delete flag - archived categories are hidden but preserved for history.
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// Display order within the parent level.
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Foreign key to parent category (null for root categories).
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Navigation property to parent category.
    /// </summary>
    public Category? Parent { get; set; }

    /// <summary>
    /// Navigation property to child categories.
    /// </summary>
    public ICollection<Category> Children { get; set; } = new List<Category>();

    /// <summary>
    /// Navigation property to transactions in this category.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
