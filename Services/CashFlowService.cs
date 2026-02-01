using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class CashFlowService : ICashFlowService
{
    private readonly ApplicationDbContext _context;
    private readonly IRecurringTransactionService _recurringService;

    public CashFlowService(ApplicationDbContext context, IRecurringTransactionService recurringService)
    {
        _context = context;
        _recurringService = recurringService;
    }

    public async Task<CashFlowForecast> GetForecastAsync(int daysAhead = 90)
    {
        var startDate = DateTime.Today;
        var endDate = startDate.AddDays(daysAhead);

        // Get current balance
        var currentBalance = await GetCurrentBalanceAsync();

        // Get projected transactions
        var projectedTransactions = await GetProjectedTransactionsAsync(startDate, endDate);

        // Calculate daily balances
        var dailyBalances = CalculateDailyBalances(currentBalance, projectedTransactions, startDate, endDate);

        // Calculate totals
        var totalIncome = projectedTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpenses = projectedTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var netCashFlow = totalIncome - totalExpenses;
        var projectedEndBalance = currentBalance + netCashFlow;

        // Generate alerts
        var alerts = GenerateAlerts(dailyBalances, projectedTransactions);

        return new CashFlowForecast(
            startDate,
            endDate,
            currentBalance,
            projectedEndBalance,
            totalIncome,
            totalExpenses,
            netCashFlow,
            dailyBalances,
            projectedTransactions.OrderBy(t => t.Date).ToList(),
            alerts
        );
    }

    public async Task<decimal> GetProjectedBalanceAsync(DateTime targetDate)
    {
        var currentBalance = await GetCurrentBalanceAsync();
        var projectedTransactions = await GetProjectedTransactionsAsync(DateTime.Today, targetDate);

        var totalIncome = projectedTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpenses = projectedTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        return currentBalance + totalIncome - totalExpenses;
    }

    public async Task<List<ProjectedTransaction>> GetProjectedTransactionsAsync(DateTime startDate, DateTime endDate)
    {
        var projections = new List<ProjectedTransaction>();

        // 1. Get recurring transactions
        var recurringTransactions = await _recurringService.GetAllAsync();
        foreach (var recurring in recurringTransactions.Where(r => r.IsActive))
        {
            var occurrences = GetRecurringOccurrences(recurring, startDate, endDate);
            foreach (var date in occurrences)
            {
                projections.Add(new ProjectedTransaction(
                    date,
                    recurring.Name,
                    recurring.Amount,
                    recurring.Type,
                    recurring.CategoryName,
                    ProjectionSource.RecurringTransaction,
                    0.95m  // High confidence for recurring
                ));
            }
        }

        // 2. Project based on historical patterns (monthly averages)
        await AddHistoricalProjectionsAsync(projections, startDate, endDate);

        return projections;
    }

    private async Task<decimal> GetCurrentBalanceAsync()
    {
        // Sum of all fund balances
        var funds = await _context.Funds.ToListAsync();
        return funds.Sum(f => f.Balance);
    }

    private List<DateTime> GetRecurringOccurrences(RecurringTransactionDto recurring, DateTime startDate, DateTime endDate)
    {
        var occurrences = new List<DateTime>();
        
        if (!recurring.NextOccurrence.HasValue || recurring.NextOccurrence.Value > endDate)
        {
            return occurrences;
        }

        var currentDate = recurring.NextOccurrence.Value;
        
        while (currentDate <= endDate)
        {
            if (currentDate >= startDate)
            {
                occurrences.Add(currentDate);
            }

            currentDate = recurring.Pattern switch
            {
                RecurrencePattern.Daily => currentDate.AddDays(recurring.Interval),
                RecurrencePattern.Weekly => currentDate.AddDays(7 * recurring.Interval),
                RecurrencePattern.BiWeekly => currentDate.AddDays(14 * recurring.Interval),
                RecurrencePattern.Monthly => currentDate.AddMonths(recurring.Interval),
                RecurrencePattern.Quarterly => currentDate.AddMonths(3 * recurring.Interval),
                RecurrencePattern.Yearly => currentDate.AddYears(recurring.Interval),
                _ => currentDate.AddMonths(recurring.Interval)
            };

            // Safety check to prevent infinite loop
            if (occurrences.Count > 1000) break;
        }

        return occurrences;
    }

    private async Task AddHistoricalProjectionsAsync(List<ProjectedTransaction> projections, DateTime startDate, DateTime endDate)
    {
        // Get last 6 months of transactions to calculate patterns
        var historicalStartDate = DateTime.Today.AddMonths(-6);
        
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= historicalStartDate && t.Date < DateTime.Today)
            .ToListAsync();

        if (!transactions.Any()) return;

        // Calculate monthly averages by category
        var monthlyAverages = transactions
            .GroupBy(t => new { t.CategoryId, t.Type })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.Type,
                Category = g.First().Category?.Name,
                AverageAmount = g.Average(t => t.Amount),
                Count = g.Count()
            })
            .Where(x => x.Count >= 2) // Only include if at least 2 occurrences
            .ToList();

        // Project monthly averages onto future months
        var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);
        var endMonth = new DateTime(endDate.Year, endDate.Month, 1);

        while (currentMonth <= endMonth)
        {
            // Add mid-month projection for each category average
            var projectionDate = currentMonth.AddDays(14);
            
            if (projectionDate >= startDate && projectionDate <= endDate)
            {
                foreach (var avg in monthlyAverages)
                {
                    // Check if we don't already have a recurring transaction for this category this month
                    var hasRecurring = projections.Any(p => 
                        p.Date.Month == projectionDate.Month && 
                        p.Date.Year == projectionDate.Year &&
                        p.Category == avg.Category &&
                        p.Source == ProjectionSource.RecurringTransaction);

                    if (!hasRecurring)
                    {
                        projections.Add(new ProjectedTransaction(
                            projectionDate,
                            $"Estimated {avg.Category} (Historical Average)",
                            avg.AverageAmount,
                            avg.Type,
                            avg.Category,
                            ProjectionSource.HistoricalPattern,
                            0.6m  // Medium confidence for historical
                        ));
                    }
                }
            }

            currentMonth = currentMonth.AddMonths(1);
        }
    }

    private List<DailyBalance> CalculateDailyBalances(
        decimal startingBalance, 
        List<ProjectedTransaction> projections,
        DateTime startDate,
        DateTime endDate)
    {
        var dailyBalances = new List<DailyBalance>();
        var currentBalance = startingBalance;
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var dayTransactions = projections.Where(p => p.Date.Date == currentDate.Date).ToList();
            var dayIncome = dayTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var dayExpenses = dayTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            currentBalance += dayIncome - dayExpenses;

            dailyBalances.Add(new DailyBalance(
                currentDate,
                currentBalance,
                dayIncome,
                dayExpenses,
                dayTransactions.Any() ? $"{dayTransactions.Count} projected transaction(s)" : null
            ));

            currentDate = currentDate.AddDays(1);
        }

        return dailyBalances;
    }

    private List<CashFlowAlert> GenerateAlerts(List<DailyBalance> dailyBalances, List<ProjectedTransaction> projections)
    {
        var alerts = new List<CashFlowAlert>();
        const decimal lowBalanceThreshold = 5000m;
        const decimal largeExpenseThreshold = 10000m;

        // Check for low/negative balances
        foreach (var day in dailyBalances)
        {
            if (day.Balance < 0)
            {
                alerts.Add(new CashFlowAlert(
                    day.Date,
                    CashFlowAlertType.NegativeBalance,
                    $"Projected negative balance: {day.Balance:C}",
                    day.Balance
                ));
            }
            else if (day.Balance < lowBalanceThreshold)
            {
                alerts.Add(new CashFlowAlert(
                    day.Date,
                    CashFlowAlertType.LowBalance,
                    $"Low balance warning: {day.Balance:C}",
                    day.Balance
                ));
            }
        }

        // Check for large expenses
        var largeExpenses = projections
            .Where(p => p.Type == TransactionType.Expense && p.Amount > largeExpenseThreshold)
            .ToList();

        foreach (var expense in largeExpenses)
        {
            alerts.Add(new CashFlowAlert(
                expense.Date,
                CashFlowAlertType.LargeExpense,
                $"Large expense projected: {expense.Description} ({expense.Amount:C})",
                expense.Amount
            ));
        }

        return alerts.OrderBy(a => a.Date).ToList();
    }
}
