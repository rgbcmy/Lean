using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Data;
using WebUI.Data.Services;

namespace WebUI.API.Services;

/// <summary>
/// Background service for executing recurring investment plans / 定投计划自动执行后台服务
/// Checks for due plans every hour and executes them
/// 每小时检查到期的计划并执行
/// </summary>
public class RecurringPlanExecutionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringPlanExecutionService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public RecurringPlanExecutionService(
        IServiceProvider serviceProvider,
        ILogger<RecurringPlanExecutionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Plan Execution Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteDuePlansAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing recurring plans");
            }

            // Wait for next check interval or cancellation
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Service is stopping
                break;
            }
        }

        _logger.LogInformation("Recurring Plan Execution Service stopped");
    }

    private async Task ExecuteDuePlansAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebUIDbContext>();
        var tradingService = scope.ServiceProvider.GetRequiredService<ITradingService>();

        // Get all active plans that are due for execution
        var now = DateTime.UtcNow;
        var duePlans = await dbContext.RecurringPlans
            .Include(p => p.BrokerAccount)
            .Where(p =>
                p.Status == "Active" &&
                p.NextExecutionDate <= now &&
                (!p.EndDate.HasValue || p.EndDate.Value >= now))
            .ToListAsync(cancellationToken);

        if (duePlans.Count == 0)
        {
            _logger.LogInformation("No recurring plans due for execution");
            return;
        }

        _logger.LogInformation("Found {Count} recurring plans due for execution", duePlans.Count);

        foreach (var plan in duePlans)
        {
            try
            {
                await ExecutePlanAsync(plan, tradingService, cancellationToken);
                
                // Update plan after successful execution
                plan.ExecutionCount++;
                plan.LastExecutionDate = DateTime.UtcNow;
                plan.LastExecutionStatus = "Success";
                plan.LastExecutionMessage = "Order submitted successfully / 订单提交成功";
                plan.NextExecutionDate = CalculateNextExecutionDate(plan.NextExecutionDate, plan.Frequency);
                plan.UpdatedAt = DateTime.UtcNow;

                // Check if plan should be completed
                if (plan.EndDate.HasValue && plan.NextExecutionDate > plan.EndDate.Value)
                {
                    plan.Status = "Completed";
                    _logger.LogInformation("Recurring plan {PlanId} completed (end date reached)", plan.Id);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully executed recurring plan {PlanId}: {Symbol}, {Amount} {Currency}",
                    plan.Id, plan.Symbol, plan.Amount, plan.Currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute recurring plan {PlanId}", plan.Id);

                // Update plan with error status
                plan.LastExecutionDate = DateTime.UtcNow;
                plan.LastExecutionStatus = "Failed";
                plan.LastExecutionMessage = $"Error: {ex.Message}";
                plan.UpdatedAt = DateTime.UtcNow;

                // Still update next execution date to avoid retrying immediately
                plan.NextExecutionDate = CalculateNextExecutionDate(plan.NextExecutionDate, plan.Frequency);

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Failed to save error status for recurring plan {PlanId}", plan.Id);
                }
            }
        }
    }

    private async Task ExecutePlanAsync(Data.Entities.RecurringPlan plan, ITradingService tradingService, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing recurring plan {PlanId}: Buying {Amount} {Currency} of {Symbol}",
            plan.Id, plan.Amount, plan.Currency, plan.Symbol);

        // Get current ETF price to calculate quantity
        // For now, we'll submit a market order with the amount in dollars
        // The trading system should convert this to shares

        // Create a market order request
        // Note: In a real implementation, we'd need to get the current price
        // and calculate the quantity. For MVP, we'll use a simple approach:
        // Submit a fractional share order or calculate shares based on last price

        // Get approximate share quantity (this should be enhanced with real-time pricing)
        var quantity = await CalculateShareQuantityAsync(plan.Symbol, plan.Amount, tradingService);

        if (quantity < 1)
        {
            throw new InvalidOperationException($"Insufficient amount {plan.Amount} to buy at least 1 share of {plan.Symbol}");
        }

        var orderRequest = new MarketOrderRequest
        {
            Symbol = plan.Symbol,
            Side = "Buy",
            Quantity = quantity
        };

        // Submit the market order
        var result = await tradingService.SubmitMarketOrderAsync(plan.BrokerAccountId, orderRequest);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Order submission failed: {result.Message}");
        }

        _logger.LogInformation("Market order submitted for recurring plan {PlanId}: OrderId={OrderId}, {Quantity} shares of {Symbol}",
            plan.Id, result.OrderId, quantity, plan.Symbol);
    }

    private async Task<int> CalculateShareQuantityAsync(string symbol, decimal amount, ITradingService tradingService)
    {
        // Try to get current price from market data
        // For MVP, use a simplified calculation
        // In production, this should use real-time market data

        try
        {
            // Get a quote (this would come from market data service)
            // For now, use a conservative estimate
            // Assuming average price of $100 per share
            var estimatedPrice = 100m;

            // Calculate shares, rounded down
            var shares = (int)Math.Floor(amount / estimatedPrice);

            return Math.Max(1, shares); // At least 1 share
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get price for {Symbol}, using minimum quantity", symbol);
            return 1; // Default to 1 share
        }
    }

    private DateTime CalculateNextExecutionDate(DateTime fromDate, string frequency)
    {
        return frequency switch
        {
            "Weekly" => fromDate.AddDays(7),
            "BiWeekly" => fromDate.AddDays(14),
            "Monthly" => fromDate.AddMonths(1),
            _ => fromDate.AddMonths(1) // Default to monthly
        };
    }
}
