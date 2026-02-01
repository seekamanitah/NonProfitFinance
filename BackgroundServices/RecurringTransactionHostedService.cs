using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NonProfitFinance.Services;

namespace NonProfitFinance.BackgroundServices;

/// <summary>
/// Background service that automatically processes recurring transactions daily at midnight
/// </summary>
public class RecurringTransactionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringTransactionHostedService> _logger;
    private Timer? _timer;
    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan[] RetryDelays = { 
        TimeSpan.FromSeconds(5), 
        TimeSpan.FromSeconds(30), 
        TimeSpan.FromMinutes(2) 
    };

    public RecurringTransactionHostedService(
        IServiceProvider serviceProvider,
        ILogger<RecurringTransactionHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Transaction Hosted Service started");

        // Calculate time until next midnight
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1);
        var initialDelay = nextMidnight - now;

        // Schedule first run at midnight, then every 24 hours
        _timer = new Timer(
            async _ => await ProcessWithRetryAsync(),
            null,
            initialDelay,
            TimeSpan.FromHours(24)
        );

        return Task.CompletedTask;
    }

    private async Task ProcessWithRetryAsync()
    {
        for (int attempt = 0; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                await ProcessRecurringTransactionsAsync();
                return; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                if (attempt < MaxRetryAttempts)
                {
                    var delay = RetryDelays[attempt];
                    _logger.LogWarning(ex, 
                        "Recurring transaction processing failed (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}...",
                        attempt + 1, MaxRetryAttempts, delay);
                    await Task.Delay(delay);
                }
                else
                {
                    _logger.LogError(ex, 
                        "Recurring transaction processing failed after {MaxAttempts} attempts. Manual intervention required.",
                        MaxRetryAttempts);
                }
            }
        }
    }

    private async Task ProcessRecurringTransactionsAsync()
    {
        _logger.LogInformation("Starting automatic recurring transaction processing at {Time}", DateTime.Now);

        using var scope = _serviceProvider.CreateScope();
        var recurringService = scope.ServiceProvider.GetRequiredService<IRecurringTransactionService>();

        var result = await recurringService.ProcessDueTransactionsAsync();

        _logger.LogInformation(
            "Recurring transactions processed successfully. Created: {Created}, Failed: {Failed}",
            result.SuccessCount,
            result.FailedCount
        );
    }

    private async void ProcessRecurringTransactions(object? state)
    {
        await ProcessWithRetryAsync();
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Transaction Hosted Service stopped");
        _timer?.Change(Timeout.Infinite, 0);
        return base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
