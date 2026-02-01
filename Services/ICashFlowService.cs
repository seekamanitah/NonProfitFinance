using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface ICashFlowService
{
    /// <summary>
    /// Generate cash flow forecast based on recurring transactions and historical patterns
    /// </summary>
    Task<CashFlowForecast> GetForecastAsync(int daysAhead = 90);
    
    /// <summary>
    /// Get projected balance for a specific date
    /// </summary>
    Task<decimal> GetProjectedBalanceAsync(DateTime targetDate);
    
    /// <summary>
    /// Get all projected transactions for the forecast period
    /// </summary>
    Task<List<ProjectedTransaction>> GetProjectedTransactionsAsync(DateTime startDate, DateTime endDate);
}

public record CashFlowForecast(
    DateTime StartDate,
    DateTime EndDate,
    decimal CurrentBalance,
    decimal ProjectedEndBalance,
    decimal TotalProjectedIncome,
    decimal TotalProjectedExpenses,
    decimal NetProjectedCashFlow,
    List<DailyBalance> DailyBalances,
    List<ProjectedTransaction> UpcomingTransactions,
    List<CashFlowAlert> Alerts
);

public record DailyBalance(
    DateTime Date,
    decimal Balance,
    decimal Income,
    decimal Expenses,
    string? Notes
);

public record ProjectedTransaction(
    DateTime Date,
    string Description,
    decimal Amount,
    TransactionType Type,
    string? Category,
    ProjectionSource Source,
    decimal Confidence  // 0.0 to 1.0
);

public record CashFlowAlert(
    DateTime Date,
    CashFlowAlertType Type,
    string Message,
    decimal? Amount
);

public enum ProjectionSource
{
    RecurringTransaction,
    HistoricalPattern,
    AverageExpense,
    ScheduledPayment
}

public enum CashFlowAlertType
{
    LowBalance,
    NegativeBalance,
    LargeExpense,
    HighCashPosition
}
