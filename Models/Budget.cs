namespace NonProfitFinance.Models;

/// <summary>
/// Represents a budget for a fiscal year
/// </summary>
public class Budget
{
    public int Id { get; set; }
    
    /// <summary>
    /// Fiscal year for this budget
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// Budget name/description
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Total budgeted amount
    /// </summary>
    public decimal TotalBudget { get; set; }
    
    /// <summary>
    /// Total actual spending
    /// </summary>
    public decimal TotalSpent { get; set; }
    
    /// <summary>
    /// Remaining budget
    /// </summary>
    public decimal Remaining { get; set; }
    
    /// <summary>
    /// Percentage of budget used
    /// </summary>
    public decimal PercentageUsed { get; set; }
    
    /// <summary>
    /// Line items for each category
    /// </summary>
    public ICollection<BudgetLineItem> LineItems { get; set; } = new List<BudgetLineItem>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents a budget line item for a specific category
/// </summary>
public class BudgetLineItem
{
    public int Id { get; set; }
    
    /// <summary>
    /// Associated budget
    /// </summary>
    public int BudgetId { get; set; }
    public Budget? Budget { get; set; }
    
    /// <summary>
    /// Category this line item is for
    /// </summary>
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public string CategoryName { get; set; } = "";
    public string? CategoryColor { get; set; }
    
    /// <summary>
    /// Budgeted amount for this category
    /// </summary>
    public decimal BudgetAmount { get; set; }
    
    /// <summary>
    /// Actual spending for this category
    /// </summary>
    public decimal ActualAmount { get; set; }
    
    /// <summary>
    /// Variance (positive = under budget, negative = over budget)
    /// </summary>
    public decimal Variance { get; set; }
    
    /// <summary>
    /// Percentage of budget used for this category
    /// </summary>
    public decimal PercentageUsed { get; set; }
}
