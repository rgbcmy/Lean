using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Core.Services;
using WebUI.Data.Entities;

namespace WebUI.Data.Services;

/// <summary>
/// ETF service implementation / ETF 服务实现
/// </summary>
public class EtfService : IEtfService
{
    private readonly ILogger<EtfService> _logger;
    private readonly IMarketDataService _marketDataService;
    private readonly WebUIDbContext _dbContext;

    // Mock ETF database for MVP - in production, this would come from a real data provider
    // 模拟 ETF 数据库（MVP）- 生产环境中应从真实数据提供商获取
    private static readonly List<EtfSearchResult> _mockEtfDatabase = new()
    {
        new EtfSearchResult
        {
            Symbol = "SPY",
            Name = "SPDR S&P 500 ETF Trust",
            Exchange = "NYSEARCA",
            TrackingIndex = "S&P 500",
            Issuer = "State Street",
            Category = "Large Cap Equity",
            ExpenseRatio = 0.0945m,
            Aum = 450_000_000_000m,
            AvgVolume = 75_000_000
        },
        new EtfSearchResult
        {
            Symbol = "VOO",
            Name = "Vanguard S&P 500 ETF",
            Exchange = "NYSEARCA",
            TrackingIndex = "S&P 500",
            Issuer = "Vanguard",
            Category = "Large Cap Equity",
            ExpenseRatio = 0.03m,
            Aum = 350_000_000_000m,
            AvgVolume = 5_000_000
        },
        new EtfSearchResult
        {
            Symbol = "QQQ",
            Name = "Invesco QQQ Trust",
            Exchange = "NASDAQ",
            TrackingIndex = "NASDAQ-100",
            Issuer = "Invesco",
            Category = "Technology",
            ExpenseRatio = 0.20m,
            Aum = 200_000_000_000m,
            AvgVolume = 45_000_000
        },
        new EtfSearchResult
        {
            Symbol = "IWM",
            Name = "iShares Russell 2000 ETF",
            Exchange = "NYSEARCA",
            TrackingIndex = "Russell 2000",
            Issuer = "BlackRock",
            Category = "Small Cap Equity",
            ExpenseRatio = 0.19m,
            Aum = 60_000_000_000m,
            AvgVolume = 30_000_000
        },
        new EtfSearchResult
        {
            Symbol = "AGG",
            Name = "iShares Core U.S. Aggregate Bond ETF",
            Exchange = "NYSEARCA",
            TrackingIndex = "Bloomberg U.S. Aggregate Bond Index",
            Issuer = "BlackRock",
            Category = "Bond",
            ExpenseRatio = 0.03m,
            Aum = 95_000_000_000m,
            AvgVolume = 6_000_000
        },
        new EtfSearchResult
        {
            Symbol = "VTI",
            Name = "Vanguard Total Stock Market ETF",
            Exchange = "NYSEARCA",
            TrackingIndex = "CRSP US Total Market",
            Issuer = "Vanguard",
            Category = "Total Market Equity",
            ExpenseRatio = 0.03m,
            Aum = 330_000_000_000m,
            AvgVolume = 4_000_000
        },
        new EtfSearchResult
        {
            Symbol = "GLD",
            Name = "SPDR Gold Shares",
            Exchange = "NYSEARCA",
            TrackingIndex = "Gold Bullion",
            Issuer = "State Street",
            Category = "Commodity",
            ExpenseRatio = 0.40m,
            Aum = 58_000_000_000m,
            AvgVolume = 7_000_000
        },
        new EtfSearchResult
        {
            Symbol = "EFA",
            Name = "iShares MSCI EAFE ETF",
            Exchange = "NYSEARCA",
            TrackingIndex = "MSCI EAFE",
            Issuer = "BlackRock",
            Category = "International Equity",
            ExpenseRatio = 0.32m,
            Aum = 70_000_000_000m,
            AvgVolume = 20_000_000
        }
    };

    public EtfService(ILogger<EtfService> logger, IMarketDataService marketDataService, WebUIDbContext dbContext)
    {
        _logger = logger;
        _marketDataService = marketDataService;
        _dbContext = dbContext;
    }

    public async Task<EtfSearchResponse> SearchEtfsAsync(EtfSearchRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching ETFs with query: {Query}, category: {Category}", request.Query, request.Category);

        var query = _mockEtfDatabase.AsQueryable();

        // Filter by search query
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var searchTerm = request.Query.ToUpper();
            query = query.Where(e =>
                e.Symbol.ToUpper().Contains(searchTerm) ||
                e.Name.ToUpper().Contains(searchTerm) ||
                (e.TrackingIndex != null && e.TrackingIndex.ToUpper().Contains(searchTerm)));
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(e => e.Category != null && e.Category.Equals(request.Category, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by issuer
        if (!string.IsNullOrWhiteSpace(request.Issuer))
        {
            query = query.Where(e => e.Issuer != null && e.Issuer.Equals(request.Issuer, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by minimum volume
        if (request.MinVolume.HasValue)
        {
            query = query.Where(e => e.AvgVolume.HasValue && e.AvgVolume.Value >= request.MinVolume.Value);
        }

        // Filter by maximum expense ratio
        if (request.MaxExpenseRatio.HasValue)
        {
            query = query.Where(e => e.ExpenseRatio.HasValue && e.ExpenseRatio.Value <= request.MaxExpenseRatio.Value);
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        var totalCount = query.Count();

        // Apply pagination
        var results = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Enrich with current prices (async)
        await EnrichWithPricesAsync(results, cancellationToken);

        return new EtfSearchResponse
        {
            Results = results,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<EtfDetail?> GetEtfDetailAsync(string symbol, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting ETF details for {Symbol}", symbol);

        var etf = _mockEtfDatabase.FirstOrDefault(e => e.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        if (etf == null)
        {
            _logger.LogWarning("ETF not found: {Symbol}", symbol);
            return null;
        }

        // Get current price
        var quote = await _marketDataService.GetQuoteAsync(symbol, cancellationToken);

        var detail = new EtfDetail
        {
            Symbol = etf.Symbol,
            Name = etf.Name,
            Exchange = etf.Exchange,
            TrackingIndex = etf.TrackingIndex,
            Issuer = etf.Issuer,
            Category = etf.Category,
            Aum = etf.Aum,
            ExpenseRatio = etf.ExpenseRatio,
            AvgVolume = etf.AvgVolume,
            LastPrice = quote?.LastPrice,
            DayChange = quote?.ChangePercent,
            InceptionDate = GetMockInceptionDate(symbol),
            Holdings = GetMockHoldings(symbol),
            DividendYield = GetMockDividendYield(symbol),
            Description = GetMockDescription(symbol),
            // Performance data would come from historical price data in production
            WeekReturn = GetMockReturn(symbol, "week"),
            MonthReturn = GetMockReturn(symbol, "month"),
            YearReturn = GetMockReturn(symbol, "year")
        };

        return detail;
    }

    public async Task<EtfCompareResponse> CompareEtfsAsync(EtfCompareRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Comparing ETFs: {Symbols}", string.Join(", ", request.Symbols));

        var comparisons = new List<EtfComparisonItem>();

        foreach (var symbol in request.Symbols)
        {
            var detail = await GetEtfDetailAsync(symbol, cancellationToken);
            if (detail != null)
            {
                comparisons.Add(new EtfComparisonItem
                {
                    Symbol = detail.Symbol,
                    Name = detail.Name,
                    ExpenseRatio = detail.ExpenseRatio,
                    Aum = detail.Aum,
                    TrackingError = GetMockTrackingError(symbol),
                    YearReturn = detail.YearReturn,
                    DividendYield = detail.DividendYield
                });
            }
        }

        var response = new EtfCompareResponse
        {
            Comparisons = comparisons
        };

        // Add performance data if requested
        if (request.IncludePerformanceData)
        {
            response.PerformanceData = GetMockPerformanceData(request.Symbols, request.PerformancePeriodDays);
        }

        return response;
    }

    public Task<EtfDividendResponse> GetEtfDividendsAsync(string symbol, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting dividend information for {Symbol}", symbol);

        var dividends = GetMockDividends(symbol);
        var totalDividends = dividends.Sum(d => d.Amount);

        // Get current price for yield calculation
        var etf = _mockEtfDatabase.FirstOrDefault(e => e.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        var currentPrice = etf?.LastPrice ?? 100m; // Default price if not available

        var annualizedYield = currentPrice > 0 ? (totalDividends / currentPrice) * 100 : 0;

        var response = new EtfDividendResponse
        {
            Symbol = symbol,
            Dividends = dividends,
            TotalDividends = totalDividends,
            AnnualizedYield = annualizedYield
        };

        return Task.FromResult(response);
    }

    // Helper methods

    private IQueryable<EtfSearchResult> ApplySorting(IQueryable<EtfSearchResult> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "expenseratio" => isDescending ? query.OrderByDescending(e => e.ExpenseRatio) : query.OrderBy(e => e.ExpenseRatio),
            "aum" => isDescending ? query.OrderByDescending(e => e.Aum) : query.OrderBy(e => e.Aum),
            "volume" => isDescending ? query.OrderByDescending(e => e.AvgVolume) : query.OrderBy(e => e.AvgVolume),
            _ => query.OrderBy(e => e.Symbol) // Default sort by symbol
        };
    }

    private async Task EnrichWithPricesAsync(List<EtfSearchResult> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            try
            {
                var quote = await _marketDataService.GetQuoteAsync(result.Symbol, cancellationToken);
                if (quote != null)
                {
                    result.LastPrice = quote.LastPrice;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get price for {Symbol}", result.Symbol);
            }
        }
    }

    // Mock data helpers (would be replaced with real data provider in production)

    private DateTime GetMockInceptionDate(string symbol)
    {
        return symbol switch
        {
            "SPY" => new DateTime(1993, 1, 22),
            "VOO" => new DateTime(2010, 9, 7),
            "QQQ" => new DateTime(1999, 3, 10),
            "IWM" => new DateTime(2000, 5, 22),
            "AGG" => new DateTime(2003, 9, 22),
            "VTI" => new DateTime(2001, 5, 24),
            "GLD" => new DateTime(2004, 11, 18),
            "EFA" => new DateTime(2001, 8, 14),
            _ => DateTime.UtcNow.AddYears(-5)
        };
    }

    private List<EtfHolding> GetMockHoldings(string symbol)
    {
        // Mock holdings data
        return symbol switch
        {
            "SPY" or "VOO" => new List<EtfHolding>
            {
                new() { Symbol = "AAPL", Name = "Apple Inc.", Weight = 7.2m },
                new() { Symbol = "MSFT", Name = "Microsoft Corporation", Weight = 6.8m },
                new() { Symbol = "AMZN", Name = "Amazon.com Inc.", Weight = 3.5m },
                new() { Symbol = "NVDA", Name = "NVIDIA Corporation", Weight = 3.2m },
                new() { Symbol = "GOOGL", Name = "Alphabet Inc. Class A", Weight = 2.1m }
            },
            "QQQ" => new List<EtfHolding>
            {
                new() { Symbol = "AAPL", Name = "Apple Inc.", Weight = 12.3m },
                new() { Symbol = "MSFT", Name = "Microsoft Corporation", Weight = 11.7m },
                new() { Symbol = "AMZN", Name = "Amazon.com Inc.", Weight = 6.2m },
                new() { Symbol = "NVDA", Name = "NVIDIA Corporation", Weight = 5.8m },
                new() { Symbol = "META", Name = "Meta Platforms Inc.", Weight = 4.5m }
            },
            _ => new List<EtfHolding>()
        };
    }

    private decimal? GetMockDividendYield(string symbol)
    {
        return symbol switch
        {
            "SPY" => 1.45m,
            "VOO" => 1.48m,
            "QQQ" => 0.62m,
            "IWM" => 1.35m,
            "AGG" => 2.87m,
            "VTI" => 1.52m,
            "EFA" => 2.45m,
            _ => null
        };
    }

    private string GetMockDescription(string symbol)
    {
        return symbol switch
        {
            "SPY" => "SPDR S&P 500 ETF Trust seeks to provide investment results that correspond to the price and yield performance of the S&P 500 Index.",
            "VOO" => "Vanguard S&P 500 ETF seeks to track the performance of the S&P 500 Index with low expense ratio.",
            "QQQ" => "Invesco QQQ Trust tracks the NASDAQ-100 Index, which includes 100 of the largest domestic and international non-financial companies.",
            "IWM" => "iShares Russell 2000 ETF seeks to track the investment results of the Russell 2000 Index, a small-capitalization index.",
            "AGG" => "iShares Core U.S. Aggregate Bond ETF tracks the Bloomberg U.S. Aggregate Bond Index, providing broad exposure to U.S. investment grade bonds.",
            "VTI" => "Vanguard Total Stock Market ETF seeks to track the performance of the CRSP US Total Market Index.",
            "GLD" => "SPDR Gold Shares seeks to reflect the performance of the price of gold bullion, less the Trust's expenses.",
            "EFA" => "iShares MSCI EAFE ETF seeks to track the investment results of the MSCI EAFE Index, providing exposure to developed markets outside North America.",
            _ => $"ETF tracking information for {symbol}"
        };
    }

    private decimal? GetMockReturn(string symbol, string period)
    {
        // Mock performance returns
        var random = new Random(symbol.GetHashCode());
        return period switch
        {
            "week" => (decimal)(random.NextDouble() * 4 - 2), // -2% to +2%
            "month" => (decimal)(random.NextDouble() * 8 - 4), // -4% to +4%
            "year" => (decimal)(random.NextDouble() * 30 - 5), // -5% to +25%
            _ => null
        };
    }

    private decimal GetMockTrackingError(string symbol)
    {
        return symbol switch
        {
            "SPY" => 0.02m,
            "VOO" => 0.01m,
            "QQQ" => 0.03m,
            _ => 0.05m
        };
    }

    private Dictionary<string, List<PerformanceDataPoint>> GetMockPerformanceData(List<string> symbols, int days)
    {
        var result = new Dictionary<string, List<PerformanceDataPoint>>();

        foreach (var symbol in symbols)
        {
            var data = new List<PerformanceDataPoint>();
            var random = new Random(symbol.GetHashCode());
            var value = 100m;

            for (int i = 0; i < days; i++)
            {
                var change = (decimal)((random.NextDouble() - 0.5) * 2); // -1% to +1% daily
                value *= (1 + change / 100);
                data.Add(new PerformanceDataPoint
                {
                    Date = DateTime.UtcNow.AddDays(-days + i),
                    Value = value
                });
            }

            result[symbol] = data;
        }

        return result;
    }

    private List<EtfDividend> GetMockDividends(string symbol)
    {
        // Mock dividend data for the last 12 months
        var hasQuarterlyDividends = symbol is "SPY" or "VOO" or "VTI" or "AGG" or "EFA";
        if (!hasQuarterlyDividends) return new List<EtfDividend>();

        var dividends = new List<EtfDividend>();
        var baseAmount = symbol switch
        {
            "SPY" => 1.5m,
            "VOO" => 1.4m,
            "AGG" => 0.5m,
            "VTI" => 0.8m,
            "EFA" => 0.6m,
            _ => 0.5m
        };

        // Generate quarterly dividends for the last 12 months
        for (int i = 0; i < 4; i++)
        {
            dividends.Add(new EtfDividend
            {
                ExDate = DateTime.UtcNow.AddMonths(-i * 3),
                PaymentDate = DateTime.UtcNow.AddMonths(-i * 3).AddDays(5),
                Amount = baseAmount * (1 + (decimal)(new Random().NextDouble() * 0.1) - 0.05m) // +/- 5% variation
            });
        }

        return dividends.OrderByDescending(d => d.ExDate).ToList();
    }

    // Recurring Investment Plan methods

    public async Task<RecurringPlanResponse> CreateRecurringPlanAsync(int userId, int brokerAccountId, RecurringPlanRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating recurring plan for user {UserId}: {Symbol}, {Amount} {Currency}, {Frequency}",
            userId, request.Symbol, request.Amount, request.Currency, request.Frequency);

        // Calculate next execution date based on start date and frequency
        var nextExecutionDate = CalculateNextExecutionDate(request.StartDate, request.Frequency);

        var plan = new RecurringPlan
        {
            UserId = userId,
            BrokerAccountId = brokerAccountId,
            Symbol = request.Symbol,
            Name = request.Name ?? request.Symbol,
            Amount = request.Amount,
            Currency = request.Currency,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextExecutionDate = nextExecutionDate,
            Status = "Active",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.RecurringPlans.Add(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recurring plan created: ID={PlanId}, User={UserId}, Symbol={Symbol}",
            plan.Id, userId, request.Symbol);

        return MapToResponse(plan);
    }

    public async Task<List<RecurringPlanResponse>> GetRecurringPlansAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recurring plans for user {UserId}", userId);

        var plans = await _dbContext.RecurringPlans
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return plans.Select(MapToResponse).ToList();
    }

    public async Task<RecurringPlanResponse?> GetRecurringPlanAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recurring plan {PlanId} for user {UserId}", planId, userId);

        var plan = await _dbContext.RecurringPlans
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        return plan == null ? null : MapToResponse(plan);
    }

    public async Task<RecurringPlanResponse> UpdateRecurringPlanAsync(int userId, int planId, UpdateRecurringPlanRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating recurring plan {PlanId} for user {UserId}", planId, userId);

        var plan = await _dbContext.RecurringPlans
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        if (plan == null)
        {
            throw new InvalidOperationException($"Recurring plan {planId} not found for user {userId}");
        }

        // Update fields if provided
        if (request.Amount.HasValue)
            plan.Amount = request.Amount.Value;

        if (!string.IsNullOrWhiteSpace(request.Frequency))
        {
            plan.Frequency = request.Frequency;
            // Recalculate next execution date based on new frequency
            plan.NextExecutionDate = CalculateNextExecutionDate(DateTime.UtcNow, request.Frequency);
        }

        if (request.EndDate.HasValue)
            plan.EndDate = request.EndDate.Value;

        if (!string.IsNullOrWhiteSpace(request.Notes))
            plan.Notes = request.Notes;

        if (!string.IsNullOrWhiteSpace(request.Status))
            plan.Status = request.Status;

        plan.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recurring plan updated: ID={PlanId}, Status={Status}", planId, plan.Status);

        return MapToResponse(plan);
    }

    public async Task DeleteRecurringPlanAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting recurring plan {PlanId} for user {UserId}", planId, userId);

        var plan = await _dbContext.RecurringPlans
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        if (plan == null)
        {
            throw new InvalidOperationException($"Recurring plan {planId} not found for user {userId}");
        }

        _dbContext.RecurringPlans.Remove(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recurring plan deleted: ID={PlanId}", planId);
    }

    private RecurringPlanResponse MapToResponse(RecurringPlan plan)
    {
        return new RecurringPlanResponse
        {
            Id = plan.Id,
            Symbol = plan.Symbol,
            Name = plan.Name,
            Amount = plan.Amount,
            Currency = plan.Currency,
            Frequency = plan.Frequency,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            NextExecutionDate = plan.NextExecutionDate,
            Status = plan.Status,
            ExecutionCount = plan.ExecutionCount,
            LastExecutionDate = plan.LastExecutionDate,
            LastExecutionStatus = plan.LastExecutionStatus,
            LastExecutionMessage = plan.LastExecutionMessage,
            Notes = plan.Notes,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt
        };
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
