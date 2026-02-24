using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Core.Services;

namespace WebUI.Data.Services;

/// <summary>
/// Risk management service implementation / 风险管理服务实现
/// </summary>
public class RiskService : IRiskService
{
    private readonly WebUIDbContext _context;
    private readonly IPortfolioService _portfolioService;
    private readonly ILogger<RiskService> _logger;

    public RiskService(
        WebUIDbContext context,
        IPortfolioService portfolioService,
        ILogger<RiskService> logger)
    {
        _context = context;
        _portfolioService = portfolioService;
        _logger = logger;
    }

    public async Task<RiskConfig?> GetRiskConfigAsync(int brokerAccountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting risk config for broker account {BrokerAccountId}", brokerAccountId);

        // In a real implementation, this would fetch from database
        // For now, return a default configuration
        return new RiskConfig
        {
            Id = 1,
            BrokerAccountId = brokerAccountId,
            StopLoss = new StopLossConfig { Enabled = false, Percentage = 5.0m },
            TakeProfit = new TakeProfitConfig { Enabled = false, Percentage = 10.0m },
            PositionLimits = new PositionLimitsConfig
            {
                Enabled = true,
                MaxPositionValuePerStock = 10000m,
                MaxPercentagePerStock = 10.0m,
                MaxTotalPositions = 20,
                MinCashRatio = 10.0m
            },
            TradingFrequency = new TradingFrequencyConfig
            {
                Enabled = true,
                MaxOrdersPerDay = 50,
                MaxDayTradesPerFiveDays = 3,
                MaxTradesPerSymbolPerDay = 10
            },
            MarginMonitoring = new MarginMonitoringConfig
            {
                Enabled = true,
                WarningThreshold = 80.0m,
                CriticalThreshold = 90.0m
            },
            Concentration = new ConcentrationConfig
            {
                Enabled = true,
                MaxSectorConcentration = 30.0m,
                MaxAssetClassConcentration = 90.0m
            },
            CircuitBreaker = new CircuitBreakerConfig
            {
                Enabled = true,
                MaxPriceMovementPercent = 20.0m,
                TimeWindowMinutes = 5,
                MaxOrderCountInWindow = 100,
                CooldownMinutes = 30
            },
            EnforcePdtRule = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<RiskConfig> SaveRiskConfigAsync(RiskConfigRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving risk config for broker account {BrokerAccountId}", request.BrokerAccountId);

        // In a real implementation, this would save to database
        var config = new RiskConfig
        {
            Id = 1,
            BrokerAccountId = request.BrokerAccountId,
            StopLoss = request.StopLoss ?? new StopLossConfig(),
            TakeProfit = request.TakeProfit ?? new TakeProfitConfig(),
            PositionLimits = request.PositionLimits ?? new PositionLimitsConfig(),
            TradingFrequency = request.TradingFrequency ?? new TradingFrequencyConfig(),
            MarginMonitoring = request.MarginMonitoring ?? new MarginMonitoringConfig(),
            Concentration = request.Concentration ?? new ConcentrationConfig(),
            CircuitBreaker = request.CircuitBreaker ?? new CircuitBreakerConfig(),
            EnforcePdtRule = request.EnforcePdtRule,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return config;
    }

    public async Task<RiskCheckResult> CheckOrderRiskAsync(
        int brokerAccountId,
        string symbol,
        string side,
        int quantity,
        decimal? price = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Checking order risk for {Symbol} {Side} {Quantity} @ {Price}",
            symbol, side, quantity, price);

        var result = new RiskCheckResult { IsAllowed = true };
        var config = await GetRiskConfigAsync(brokerAccountId, cancellationToken);
        if (config == null)
        {
            return result; // No config means no restrictions
        }

        // Estimate order value
        decimal estimatedPrice = price ?? 100m; // Placeholder, should fetch current price
        decimal orderValue = quantity * estimatedPrice;

        // Check position limits
        var positionCheck = await CheckPositionLimitsAsync(brokerAccountId, symbol, orderValue, cancellationToken);
        if (!positionCheck.IsAllowed)
        {
            result.IsAllowed = false;
            result.DenialReason = positionCheck.DenialReason;
            result.Violations.AddRange(positionCheck.Violations);
            return result;
        }

        // Check trading frequency
        var frequencyCheck = await CheckTradingFrequencyAsync(brokerAccountId, symbol, cancellationToken);
        if (!frequencyCheck.IsAllowed)
        {
            result.IsAllowed = false;
            result.DenialReason = frequencyCheck.DenialReason;
            result.Violations.AddRange(frequencyCheck.Violations);
            return result;
        }

        // Check concentration
        if (side.Equals("BUY", StringComparison.OrdinalIgnoreCase))
        {
            var concentrationCheck = await CheckConcentrationAsync(brokerAccountId, symbol, orderValue, cancellationToken);
            if (!concentrationCheck.IsAllowed)
            {
                result.Warnings.AddRange(concentrationCheck.Warnings);
            }
        }

        // Check PDT rule for day trades
        bool isClosing = side.Equals("SELL", StringComparison.OrdinalIgnoreCase);
        var pdtCheck = await CheckPdtRuleAsync(brokerAccountId, symbol, isClosing, cancellationToken);
        if (pdtCheck.IsViolated && config.EnforcePdtRule)
        {
            result.IsAllowed = false;
            result.DenialReason = pdtCheck.Message;
            result.Violations.Add(new RiskViolation
            {
                Type = "PDT_VIOLATION",
                Severity = RiskSeverity.Critical,
                Message = pdtCheck.Message,
                Symbol = symbol,
                DetectedAt = DateTime.UtcNow,
                CurrentValue = pdtCheck.DayTradesCount,
                ThresholdValue = pdtCheck.MaxAllowedDayTrades
            });
            return result;
        }

        return result;
    }

    public async Task MonitorPositionsAsync(int brokerAccountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Monitoring positions for broker account {BrokerAccountId}", brokerAccountId);

        var config = await GetRiskConfigAsync(brokerAccountId, cancellationToken);
        if (config == null) return;

        var positions = await _portfolioService.GetPositionsAsync(brokerAccountId, cancellationToken);

        foreach (var position in positions.Positions)
        {
            // Check stop-loss
            if (config.StopLoss.Enabled)
            {
                var stopLossPct = config.StopLoss.SymbolOverrides.ContainsKey(position.Symbol)
                    ? config.StopLoss.SymbolOverrides[position.Symbol]
                    : config.StopLoss.Percentage;

                if (position.UnrealizedPnLPercent <= -stopLossPct)
                {
                    _logger.LogWarning(
                        "Stop-loss triggered for {Symbol}: P&L {PnL}% <= -{StopLoss}%",
                        position.Symbol, position.UnrealizedPnLPercent, stopLossPct);

                    await ExecuteStopLossAsync(brokerAccountId, position.Symbol, cancellationToken);
                }
            }

            // Check take-profit
            if (config.TakeProfit.Enabled)
            {
                var takeProfitPct = config.TakeProfit.SymbolOverrides.ContainsKey(position.Symbol)
                    ? config.TakeProfit.SymbolOverrides[position.Symbol]
                    : config.TakeProfit.Percentage;

                if (position.UnrealizedPnLPercent >= takeProfitPct)
                {
                    _logger.LogInformation(
                        "Take-profit triggered for {Symbol}: P&L {PnL}% >= {TakeProfit}%",
                        position.Symbol, position.UnrealizedPnLPercent, takeProfitPct);

                    await ExecuteTakeProfitAsync(brokerAccountId, position.Symbol, cancellationToken);
                }
            }
        }
    }

    public async Task<RiskCheckResult> CheckPositionLimitsAsync(
        int brokerAccountId,
        string symbol,
        decimal orderValue,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking position limits for {Symbol} order value {OrderValue}", symbol, orderValue);

        var result = new RiskCheckResult { IsAllowed = true };
        var config = await GetRiskConfigAsync(brokerAccountId, cancellationToken);
        if (config?.PositionLimits.Enabled != true)
        {
            return result;
        }

        var positions = await _portfolioService.GetPositionsAsync(brokerAccountId, cancellationToken);
        var allocation = await _portfolioService.GetPortfolioAllocationAsync(brokerAccountId, cancellationToken);

        // Check max positions count
        if (positions.Positions.Count >= config.PositionLimits.MaxTotalPositions &&
            !positions.Positions.Any(p => p.Symbol == symbol))
        {
            result.IsAllowed = false;
            result.DenialReason = $"Maximum position count reached: {config.PositionLimits.MaxTotalPositions}";
            result.Violations.Add(new RiskViolation
            {
                Type = "MAX_POSITIONS_EXCEEDED",
                Severity = RiskSeverity.High,
                Message = result.DenialReason,
                DetectedAt = DateTime.UtcNow,
                CurrentValue = positions.Positions.Count,
                ThresholdValue = config.PositionLimits.MaxTotalPositions
            });
            return result;
        }

        // Check max value per stock
        if (config.PositionLimits.MaxPositionValuePerStock.HasValue)
        {
            var existingPosition = positions.Positions.FirstOrDefault(p => p.Symbol == symbol);
            decimal totalValue = (existingPosition?.MarketValue ?? 0) + orderValue;

            if (totalValue > config.PositionLimits.MaxPositionValuePerStock.Value)
            {
                result.IsAllowed = false;
                result.DenialReason = $"Position value limit exceeded for {symbol}: ${totalValue:N2} > ${config.PositionLimits.MaxPositionValuePerStock:N2}";
                result.Violations.Add(new RiskViolation
                {
                    Type = "MAX_POSITION_VALUE_EXCEEDED",
                    Severity = RiskSeverity.High,
                    Message = result.DenialReason,
                    Symbol = symbol,
                    DetectedAt = DateTime.UtcNow,
                    CurrentValue = totalValue,
                    ThresholdValue = config.PositionLimits.MaxPositionValuePerStock.Value
                });
                return result;
            }
        }

        // Check min cash ratio
        decimal totalEquity = allocation.TotalValue;
        decimal cashAfterOrder = allocation.Cash.Value - orderValue;
        decimal cashRatio = totalEquity > 0 ? (cashAfterOrder / totalEquity) * 100 : 0;

        if (cashRatio < config.PositionLimits.MinCashRatio)
        {
            result.IsAllowed = false;
            result.DenialReason = $"Insufficient cash reserve: {cashRatio:F2}% < {config.PositionLimits.MinCashRatio:F2}%";
            result.Violations.Add(new RiskViolation
            {
                Type = "MIN_CASH_RATIO_VIOLATION",
                Severity = RiskSeverity.High,
                Message = result.DenialReason,
                DetectedAt = DateTime.UtcNow,
                CurrentValue = cashRatio,
                ThresholdValue = config.PositionLimits.MinCashRatio
            });
            return result;
        }

        return result;
    }

    public async Task<RiskCheckResult> CheckTradingFrequencyAsync(
        int brokerAccountId,
        string symbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking trading frequency for broker account {BrokerAccountId}, symbol {Symbol}", brokerAccountId, symbol);

        var result = new RiskCheckResult { IsAllowed = true };
        var config = await GetRiskConfigAsync(brokerAccountId, cancellationToken);
        if (config?.TradingFrequency.Enabled != true)
        {
            return result;
        }

        var today = DateTime.UtcNow.Date;

        // Count orders today
        var ordersToday = await _context.Orders
            .Where(o => o.BrokerAccountId == brokerAccountId &&
                       o.CreatedAt >= today &&
                       o.CreatedAt < today.AddDays(1))
            .CountAsync(cancellationToken);

        if (ordersToday >= config.TradingFrequency.MaxOrdersPerDay)
        {
            result.IsAllowed = false;
            result.DenialReason = $"Daily order limit reached: {ordersToday}/{config.TradingFrequency.MaxOrdersPerDay}";
            result.Violations.Add(new RiskViolation
            {
                Type = "MAX_ORDERS_PER_DAY_EXCEEDED",
                Severity = RiskSeverity.Medium,
                Message = result.DenialReason,
                DetectedAt = DateTime.UtcNow,
                CurrentValue = ordersToday,
                ThresholdValue = config.TradingFrequency.MaxOrdersPerDay
            });
            return result;
        }

        // Count orders for this symbol today
        var symbolOrdersToday = await _context.Orders
            .Where(o => o.BrokerAccountId == brokerAccountId &&
                       o.Symbol == symbol &&
                       o.CreatedAt >= today &&
                       o.CreatedAt < today.AddDays(1))
            .CountAsync(cancellationToken);

        if (symbolOrdersToday >= config.TradingFrequency.MaxTradesPerSymbolPerDay)
        {
            result.IsAllowed = false;
            result.DenialReason = $"Symbol order limit reached for {symbol}: {symbolOrdersToday}/{config.TradingFrequency.MaxTradesPerSymbolPerDay}";
            result.Violations.Add(new RiskViolation
            {
                Type = "MAX_TRADES_PER_SYMBOL_EXCEEDED",
                Severity = RiskSeverity.Medium,
                Message = result.DenialReason,
                Symbol = symbol,
                DetectedAt = DateTime.UtcNow,
                CurrentValue = symbolOrdersToday,
                ThresholdValue = config.TradingFrequency.MaxTradesPerSymbolPerDay
            });
            return result;
        }

        return result;
    }

    public async Task<RiskCheckResult> CheckMarginUsageAsync(int brokerAccountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking margin usage for broker account {BrokerAccountId}", brokerAccountId);

        var result = new RiskCheckResult { IsAllowed = true };
        var config = await GetRiskConfigAsync(brokerAccountId, cancellationToken);
        if (config?.MarginMonitoring.Enabled != true)
        {
            return result;
        }

        // Placeholder: In real implementation, get margin usage from IBKR
        decimal marginUsagePercent = 0m; // TODO: Fetch from IBKR

        if (marginUsagePercent >= config.MarginMonitoring.CriticalThreshold)
        {
            result.Violations.Add(new RiskViolation
            {
                Type = "MARGIN_CRITICAL",
                Severity = RiskSeverity.Critical,
                Message = $"Margin usage critical: {marginUsagePercent:F2}% >= {config.MarginMonitoring.CriticalThreshold:F2}%",
                DetectedAt = DateTime.UtcNow,
                CurrentValue = marginUsagePercent,
                ThresholdValue = config.MarginMonitoring.CriticalThreshold
            });

            if (config.MarginMonitoring.AutoLiquidateOnCritical)
            {
                result.IsAllowed = false;
                result.DenialReason = "Margin usage critical - auto-liquidation in progress";
            }
        }
        else if (marginUsagePercent >= config.MarginMonitoring.WarningThreshold)
        {
            result.Warnings.Add(new RiskWarning
            {
                Type = "MARGIN_WARNING",
                Message = $"Margin usage warning: {marginUsagePercent:F2}% >= {config.MarginMonitoring.WarningThreshold:F2}%",
                TriggeredAt = DateTime.UtcNow
            });
        }

        return result;
    }

    public async Task<RiskCheckResult> CheckConcentrationAsync(
        int brokerAccountId,
        string symbol,
        decimal positionValue,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking concentration for {Symbol}", symbol);

        var result = new RiskCheckResult { IsAllowed = true };
        var config = await GetRiskConfigAsync(brokerAccountId, cancellationToken);
        if (config?.Concentration.Enabled != true)
        {
            return result;
        }

        var allocation = await _portfolioService.GetPortfolioAllocationAsync(brokerAccountId, cancellationToken);
        var positions = await _portfolioService.GetPositionsAsync(brokerAccountId, cancellationToken);

        var existingPosition = positions.Positions.FirstOrDefault(p => p.Symbol == symbol);
        decimal newTotalValue = (existingPosition?.MarketValue ?? 0) + positionValue;
        decimal portfolioPercentage = allocation.TotalValue > 0 ? (newTotalValue / allocation.TotalValue) * 100 : 0;

        if (portfolioPercentage > config.PositionLimits.MaxPercentagePerStock)
        {
            result.Warnings.Add(new RiskWarning
            {
                Type = "CONCENTRATION_WARNING",
                Message = $"{symbol} would be {portfolioPercentage:F2}% of portfolio (max {config.PositionLimits.MaxPercentagePerStock:F2}%)",
                Symbol = symbol,
                TriggeredAt = DateTime.UtcNow
            });
        }

        return result;
    }

    public async Task<RiskCheckResult> CheckCircuitBreakerAsync(
        int brokerAccountId,
        string symbol,
        decimal currentPrice,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking circuit breaker for {Symbol} at price {Price}", symbol, currentPrice);

        var result = new RiskCheckResult { IsAllowed = true };
        var config = await GetRiskConfigAsync(brokerAccountId, cancellationToken);
        if (config?.CircuitBreaker.Enabled != true)
        {
            return result;
        }

        // Placeholder: In real implementation, check price history from cache/database
        // For now, return allowed
        return result;
    }

    public async Task<PdtCheckResult> CheckPdtRuleAsync(
        int brokerAccountId,
        string symbol,
        bool isClosing,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking PDT rule for broker account {BrokerAccountId}, symbol {Symbol}", brokerAccountId, symbol);

        var result = new PdtCheckResult
        {
            IsViolated = false,
            MaxAllowedDayTrades = 3,
            AccountEquity = 5000m, // Placeholder
            Message = "PDT check passed"
        };

        // Get account equity from portfolio
        var allocation = await _portfolioService.GetPortfolioAllocationAsync(brokerAccountId, cancellationToken);
        result.AccountEquity = allocation.TotalValue;

        // If account equity >= $25,000, PDT rule doesn't apply
        if (result.AccountEquity >= result.MinimumEquityRequirement)
        {
            result.Message = "Account equity >= $25,000 - PDT rule does not apply";
            return result;
        }

        // Count day trades in last 5 business days
        var fiveDaysAgo = DateTime.UtcNow.AddDays(-7); // Approximate (includes weekends)
        
        // In real implementation, count actual day trades from database
        // A day trade is buying and selling the same security on the same day
        var dayTradesCount = 0; // Placeholder

        result.DayTradesCount = dayTradesCount;
        result.IsPatternDayTrader = dayTradesCount >= 4;

        if (isClosing && dayTradesCount >= result.MaxAllowedDayTrades)
        {
            result.IsViolated = true;
            result.Message = $"PDT rule violation: {dayTradesCount} day trades in last 5 business days (max {result.MaxAllowedDayTrades})";
        }

        return result;
    }

    public async Task<RiskMetrics> CalculateRiskMetricsAsync(
        int brokerAccountId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating risk metrics for broker account {BrokerAccountId}", brokerAccountId);

        // Placeholder implementation
        // In real implementation, fetch historical equity curve and calculate actual metrics
        var metrics = new RiskMetrics
        {
            ValueAtRisk = 500m, // 95% VaR, 1 day
            SharpeRatio = 1.5m,
            MaxDrawdown = 12.5m,
            CurrentDrawdown = 3.2m,
            SortinoRatio = 2.0m,
            Beta = 0.8m,
            Volatility = 15.0m,
            CalculatedAt = DateTime.UtcNow
        };

        return metrics;
    }

    public async Task<RiskReport> GenerateRiskReportAsync(int brokerAccountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating risk report for broker account {BrokerAccountId}", brokerAccountId);

        var config = await GetRiskConfigAsync(brokerAccountId, cancellationToken);
        var metrics = await CalculateRiskMetricsAsync(brokerAccountId, cancellationToken: cancellationToken);
        var positions = await _portfolioService.GetPositionsAsync(brokerAccountId, cancellationToken);
        var allocation = await _portfolioService.GetPortfolioAllocationAsync(brokerAccountId, cancellationToken);

        var report = new RiskReport
        {
            BrokerAccountId = brokerAccountId,
            GeneratedAt = DateTime.UtcNow,
            Metrics = metrics,
            Violations = new List<RiskViolation>(),
            Warnings = new List<RiskWarning>(),
            PositionRisks = new List<PositionRisk>()
        };

        // Generate position risks
        foreach (var position in positions.Positions)
        {
            var positionRisk = new PositionRisk
            {
                Symbol = position.Symbol,
                PositionValue = position.MarketValue,
                PortfolioPercentage = allocation.TotalValue > 0 ? (position.MarketValue / allocation.TotalValue) * 100 : 0,
                UnrealizedPnL = position.UnrealizedPnL,
                UnrealizedPnLPercent = position.UnrealizedPnLPercent,
                Volatility = 20.0m, // Placeholder
                RiskScore = Math.Min(100, Math.Abs(position.UnrealizedPnLPercent) * 2) // Simple calculation
            };

            // Calculate distance to stop-loss
            if (config?.StopLoss.Enabled == true)
            {
                var stopLossPct = config.StopLoss.SymbolOverrides.ContainsKey(position.Symbol)
                    ? config.StopLoss.SymbolOverrides[position.Symbol]
                    : config.StopLoss.Percentage;
                positionRisk.DistanceToStopLoss = stopLossPct + position.UnrealizedPnLPercent;
            }

            // Calculate distance to take-profit
            if (config?.TakeProfit.Enabled == true)
            {
                var takeProfitPct = config.TakeProfit.SymbolOverrides.ContainsKey(position.Symbol)
                    ? config.TakeProfit.SymbolOverrides[position.Symbol]
                    : config.TakeProfit.Percentage;
                positionRisk.DistanceToTakeProfit = takeProfitPct - position.UnrealizedPnLPercent;
            }

            report.PositionRisks.Add(positionRisk);

            // Check for warnings
            if (positionRisk.PortfolioPercentage > 15.0m)
            {
                report.Warnings.Add(new RiskWarning
                {
                    Type = "HIGH_CONCENTRATION",
                    Message = $"{position.Symbol} represents {positionRisk.PortfolioPercentage:F2}% of portfolio",
                    Symbol = position.Symbol,
                    TriggeredAt = DateTime.UtcNow
                });
            }
        }

        // Calculate overall risk score (0-100)
        decimal avgPositionRisk = report.PositionRisks.Any() 
            ? report.PositionRisks.Average(pr => pr.RiskScore) 
            : 0;
        decimal drawdownFactor = Math.Min(metrics.CurrentDrawdown * 2, 30);
        decimal volatilityFactor = Math.Min(metrics.Volatility, 30);
        
        report.OverallRiskScore = (avgPositionRisk * 0.4m) + (drawdownFactor * 0.3m) + (volatilityFactor * 0.3m);

        // Determine risk level
        report.RiskLevel = report.OverallRiskScore switch
        {
            < 20 => RiskLevel.VeryLow,
            < 40 => RiskLevel.Low,
            < 60 => RiskLevel.Medium,
            < 80 => RiskLevel.High,
            _ => RiskLevel.VeryHigh
        };

        return report;
    }

    public async Task ExecuteStopLossAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Executing stop-loss for {Symbol}", symbol);

        // In real implementation, submit market sell order through trading service
        var closeRequest = new ClosePositionRequest
        {
            OrderType = "Market"
            // Close entire position (Quantity = null means close all)
        };

        await _portfolioService.ClosePositionAsync(brokerAccountId, symbol, closeRequest, cancellationToken);
    }

    public async Task ExecuteTakeProfitAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing take-profit for {Symbol}", symbol);

        // In real implementation, submit market sell order through trading service
        var closeRequest = new ClosePositionRequest
        {
            OrderType = "Market"
            // Close entire position (Quantity = null means close all)
        };

        await _portfolioService.ClosePositionAsync(brokerAccountId, symbol, closeRequest, cancellationToken);
    }
}
