namespace NonProfitFinance.Models;

/// <summary>
/// Defines whether a category is for income or expense tracking.
/// </summary>
public enum CategoryType
{
    Income,
    Expense
}

/// <summary>
/// Defines fund restriction status for nonprofit compliance.
/// </summary>
public enum FundType
{
    /// <summary>
    /// Funds that can be used for any purpose.
    /// </summary>
    Unrestricted,

    /// <summary>
    /// Funds designated for specific purposes by donors or grantors.
    /// </summary>
    Restricted,

    /// <summary>
    /// Temporarily restricted funds with time or purpose constraints.
    /// </summary>
    TemporarilyRestricted,

    /// <summary>
    /// Permanently restricted funds (e.g., endowments).
    /// </summary>
    PermanentlyRestricted
}

/// <summary>
/// Status of a grant lifecycle.
/// </summary>
public enum GrantStatus
{
    Pending,
    Active,
    Completed,
    Expired,
    Rejected
}

/// <summary>
/// Type of donor for categorization.
/// </summary>
public enum DonorType
{
    Individual,
    SmallBusiness,
    Corporate,
    Foundation,
    Government,
    Other
}

/// <summary>
/// Transaction type for income/expense classification.
/// </summary>
public enum TransactionType
{
    Income,
    Expense,
    Transfer
}
