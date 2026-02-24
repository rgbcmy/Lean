using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;

namespace WebUI.Core.Services;

/// <summary>
/// Market data service implementation / 行情数据服务实现
/// Manages market data subscriptions, caching, and real-time updates
/// 管理市场数据订阅、缓存和实时更新
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly ILogger<MarketDataService> _logger;
    private readonly IIbkrConnectionService _ibkrService;
    private readonly ICacheService _cacheService;
    
    // Thread-safe collections for subscription management
    private readonly ConcurrentDictionary<string, MarketQuote> _quotes = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastUpdateTimes = new();
    private readonly HashSet<string> _subscribedSymbols = new();
    private readonly object _subscriptionLock = new();
    
    // Throttling configuration (1 update per second max)
    private readonly TimeSpan _updateThrottleInterval = TimeSpan.FromSeconds(1);
    
    // Cache configuration (5 minutes)
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    
    // Maximum subscriptions allowed
    private const int MaxSubscriptions = 50;
    
    // Cache key prefix
    private const string CacheKeyPrefix = "market:quote:";

    public event EventHandler<MarketDataUpdate>? OnMarketDataUpdate;

    public IReadOnlyList<string> SubscribedSymbols
    {
        get
        {
            lock (_subscriptionLock)
            {
                return _subscribedSymbols.ToList();
            }
        }
    }

    public MarketDataService(
        ILogger<MarketDataService> logger,
        IIbkrConnectionService ibkrService,
        ICacheService cacheService)
    {
        _logger = logger;
        _ibkrService = ibkrService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Subscribe to market data for symbols / 订阅股票行情
    /// </summary>
    public async Task<bool> SubscribeAsync(
        IEnumerable<string> symbols,
        bool realTime = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var symbolList = symbols.Select(s => NormalizeSymbol(s)).ToList();

            lock (_subscriptionLock)
            {
                // Check subscription limit
                if (_subscribedSymbols.Count + symbolList.Count > MaxSubscriptions)
                {
                    _logger.LogWarning("Subscription limit reached. Max: {Max}, Current: {Current}, Requested: {Requested}",
                        MaxSubscriptions, _subscribedSymbols.Count, symbolList.Count);
                    return false;
                }

                // Add to subscribed list
                foreach (var symbol in symbolList)
                {
                    _subscribedSymbols.Add(symbol);
                }
            }

            _logger.LogInformation("Subscribed to {Count} symbols: {Symbols}",
                symbolList.Count, string.Join(", ", symbolList));

            // TODO: Integrate with Lean engine to subscribe to market data
            // For now, we'll simulate by initializing quotes
            foreach (var symbol in symbolList)
            {
                if (!_quotes.ContainsKey(symbol))
                {
                    _quotes[symbol] = CreateDefaultQuote(symbol, !realTime);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to market data");
            return false;
        }
    }

    /// <summary>
    /// Unsubscribe from market data / 取消订阅行情
    /// </summary>
    public async Task<bool> UnsubscribeAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var symbolList = symbols.Select(s => NormalizeSymbol(s)).ToList();

            lock (_subscriptionLock)
            {
                foreach (var symbol in symbolList)
                {
                    _subscribedSymbols.Remove(symbol);
                }
            }

            _logger.LogInformation("Unsubscribed from {Count} symbols: {Symbols}",
                symbolList.Count, string.Join(", ", symbolList));

            // TODO: Notify Lean engine to unsubscribe

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe from market data");
            return false;
        }
    }

    /// <summary>
    /// Get current quote for a symbol / 获取股票当前行情
    /// </summary>
    public async Task<MarketQuote?> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        symbol = NormalizeSymbol(symbol);
        var cacheKey = CacheKeyPrefix + symbol;

        // Check cache first (Redis or in-memory)
        var cachedQuote = await _cacheService.GetAsync<MarketQuote>(cacheKey, cancellationToken);
        if (cachedQuote != null)
        {
            _logger.LogDebug("Returning cached quote for {Symbol}", symbol);
            return cachedQuote;
        }

        // Check in-memory dictionary (for active subscriptions)
        if (_quotes.TryGetValue(symbol, out var quote))
        {
            if (DateTime.UtcNow - quote.Timestamp < _cacheExpiration)
            {
                // Cache it for future requests
                await _cacheService.SetAsync(cacheKey, quote, _cacheExpiration, cancellationToken);
                return quote;
            }
        }

        // TODO: Fetch from Lean engine / IBKR if not cached
        _logger.LogWarning("Quote not found in cache for {Symbol}", symbol);
        return null;
    }

    /// <summary>
    /// Get quotes for multiple symbols / 获取多个股票行情
    /// </summary>
    public async Task<List<MarketQuote>> GetQuotesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        var quotes = new List<MarketQuote>();

        foreach (var symbol in symbols)
        {
            var quote = await GetQuoteAsync(symbol, cancellationToken);
            if (quote != null)
            {
                quotes.Add(quote);
            }
        }

        return quotes;
    }

    /// <summary>
    /// Get current market status / 获取当前市场状态
    /// </summary>
    public async Task<MarketStatusInfo> GetMarketStatusAsync(
        CancellationToken cancellationToken = default)
    {
        // Get current time in EST (UTC-5 or UTC-4 depending on DST)
        var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        var marketTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, estZone);

        var status = DetermineMarketStatus(marketTime);
        var isTradingDay = IsTradingDay(marketTime);

        return new MarketStatusInfo
        {
            Status = status,
            MarketTime = marketTime,
            IsTradingDay = isTradingDay,
            NextOpen = CalculateNextOpen(marketTime),
            NextClose = CalculateNextClose(marketTime)
        };
    }

    /// <summary>
    /// Get market summary with indices, movers, and sectors / 获取市场概览
    /// </summary>
    public async Task<MarketSummary> GetMarketSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual data fetching from Lean/IBKR
        // For now, return mock data structure

        var marketStatus = await GetMarketStatusAsync(cancellationToken);

        return new MarketSummary
        {
            MarketStatus = marketStatus,
            Indices = new List<IndexQuote>
            {
                new() { Symbol = "SPX", Name = "S&P 500", Value = 0, Change = 0, ChangePercent = 0 },
                new() { Symbol = "IXIC", Name = "NASDAQ", Value = 0, Change = 0, ChangePercent = 0 },
                new() { Symbol = "DJI", Name = "Dow Jones", Value = 0, Change = 0, ChangePercent = 0 }
            },
            TopGainers = new List<MarketQuote>(),
            TopLosers = new List<MarketQuote>(),
            Sectors = new List<SectorPerformance>
            {
                new() { Name = "Technology", Symbol = "XLK", ChangePercent = 0 },
                new() { Name = "Financials", Symbol = "XLF", ChangePercent = 0 },
                new() { Name = "Healthcare", Symbol = "XLV", ChangePercent = 0 },
                new() { Name = "Consumer Discretionary", Symbol = "XLY", ChangePercent = 0 },
                new() { Name = "Communication Services", Symbol = "XLC", ChangePercent = 0 },
                new() { Name = "Industrials", Symbol = "XLI", ChangePercent = 0 },
                new() { Name = "Consumer Staples", Symbol = "XLP", ChangePercent = 0 },
                new() { Name = "Energy", Symbol = "XLE", ChangePercent = 0 },
                new() { Name = "Utilities", Symbol = "XLU", ChangePercent = 0 },
                new() { Name = "Real Estate", Symbol = "XLRE", ChangePercent = 0 },
                new() { Name = "Materials", Symbol = "XLB", ChangePercent = 0 }
            }
        };
    }

    /// <summary>
    /// Update quote data (called by data feed) / 更新行情数据（由数据源调用）
    /// </summary>
    public async void UpdateQuote(MarketQuote quote)
    {
        var symbol = NormalizeSymbol(quote.Symbol);

        // Check throttling - only update if enough time has passed
        if (_lastUpdateTimes.TryGetValue(symbol, out var lastUpdate))
        {
            if (DateTime.UtcNow - lastUpdate < _updateThrottleInterval)
            {
                // Skip this update due to throttling
                return;
            }
        }

        // Update cache (both in-memory and distributed)
        quote.Timestamp = DateTime.UtcNow;
        _quotes[symbol] = quote;
        _lastUpdateTimes[symbol] = DateTime.UtcNow;

        // Update distributed cache (Redis)
        var cacheKey = CacheKeyPrefix + symbol;
        await _cacheService.SetAsync(cacheKey, quote, _cacheExpiration);

        // Fire event
        OnMarketDataUpdate?.Invoke(this, new MarketDataUpdate
        {
            Symbol = quote.Symbol,
            LastPrice = quote.LastPrice,
            BidPrice = quote.BidPrice,
            AskPrice = quote.AskPrice,
            Volume = quote.Volume,
            Change = quote.Change,
            ChangePercent = quote.ChangePercent,
            MarketStatus = "Open", // TODO: Determine actual market status
            Timestamp = quote.Timestamp
        });

        _logger.LogDebug("Updated quote for {Symbol}: Price={Price}", symbol, quote.LastPrice);
    }

    #region Helper Methods

    /// <summary>
    /// Normalize symbol to uppercase / 规范化股票代码为大写
    /// </summary>
    private string NormalizeSymbol(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Create a default quote for initialization / 创建默认行情数据
    /// </summary>
    private MarketQuote CreateDefaultQuote(string symbol, bool isDelayed)
    {
        return new MarketQuote
        {
            Symbol = symbol,
            LastPrice = 0,
            LastSize = 0,
            BidPrice = 0,
            BidSize = 0,
            AskPrice = 0,
            AskSize = 0,
            Volume = 0,
            PreviousClose = 0,
            High = 0,
            Low = 0,
            Open = 0,
            Timestamp = DateTime.UtcNow,
            IsDelayed = isDelayed
        };
    }

    /// <summary>
    /// Determine market status based on EST time / 根据美东时间判断市场状态
    /// </summary>
    private MarketStatus DetermineMarketStatus(DateTime estTime)
    {
        if (!IsTradingDay(estTime))
        {
            return MarketStatus.Closed;
        }

        var timeOfDay = estTime.TimeOfDay;

        // Pre-market: 4:00 AM - 9:30 AM EST
        if (timeOfDay >= TimeSpan.FromHours(4) && timeOfDay < TimeSpan.FromHours(9.5))
        {
            return MarketStatus.PreMarket;
        }

        // Regular hours: 9:30 AM - 4:00 PM EST
        if (timeOfDay >= TimeSpan.FromHours(9.5) && timeOfDay < TimeSpan.FromHours(16))
        {
            return MarketStatus.Open;
        }

        // After-hours: 4:00 PM - 8:00 PM EST
        if (timeOfDay >= TimeSpan.FromHours(16) && timeOfDay < TimeSpan.FromHours(20))
        {
            return MarketStatus.AfterHours;
        }

        return MarketStatus.Closed;
    }

    /// <summary>
    /// Check if it's a trading day / 检查是否为交易日
    /// </summary>
    private bool IsTradingDay(DateTime estTime)
    {
        // Not a trading day if weekend
        if (estTime.DayOfWeek == DayOfWeek.Saturday || estTime.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        // TODO: Add US market holiday checks
        // For now, assume all weekdays are trading days

        return true;
    }

    /// <summary>
    /// Calculate next market open time / 计算下次开盘时间
    /// </summary>
    private DateTime? CalculateNextOpen(DateTime estTime)
    {
        var nextOpen = estTime.Date.AddHours(9.5); // 9:30 AM

        // If market already opened today, move to next day
        if (estTime.TimeOfDay >= TimeSpan.FromHours(9.5))
        {
            nextOpen = nextOpen.AddDays(1);
        }

        // Skip weekends
        while (nextOpen.DayOfWeek == DayOfWeek.Saturday || nextOpen.DayOfWeek == DayOfWeek.Sunday)
        {
            nextOpen = nextOpen.AddDays(1);
        }

        return nextOpen;
    }

    /// <summary>
    /// Calculate next market close time / 计算下次闭市时间
    /// </summary>
    private DateTime? CalculateNextClose(DateTime estTime)
    {
        if (!IsTradingDay(estTime))
        {
            return null;
        }

        var nextClose = estTime.Date.AddHours(16); // 4:00 PM

        // If market already closed today, return null
        if (estTime.TimeOfDay >= TimeSpan.FromHours(16))
        {
            return null;
        }

        return nextClose;
    }

    /// <summary>
    /// Search for stocks by symbol or company name / 根据代码或公司名称搜索股票
    /// </summary>
    public async Task<List<StockSearchResult>> SearchStocksAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            query = query.ToUpperInvariant().Trim();
            _logger.LogInformation("Searching stocks with query: {Query}, limit: {Limit}", query, limit);

            // TODO: Integrate with actual stock data source (Alpha Vantage, IEX Cloud, or local database)
            // For now, return mock/sample data of popular stocks

            var popularStocks = new List<StockSearchResult>
            {
                new() { Symbol = "AAPL", Name = "Apple Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 180.50m },
                new() { Symbol = "MSFT", Name = "Microsoft Corporation", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 370.25m },
                new() { Symbol = "GOOGL", Name = "Alphabet Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 140.75m },
                new() { Symbol = "AMZN", Name = "Amazon.com Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 160.30m },
                new() { Symbol = "TSLA", Name = "Tesla Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 240.85m },
                new() { Symbol = "META", Name = "Meta Platforms Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 380.15m },
                new() { Symbol = "NVDA", Name = "NVIDIA Corporation", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 470.90m },
                new() { Symbol = "SPY", Name = "SPDR S&P 500 ETF Trust", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 450.60m },
                new() { Symbol = "QQQ", Name = "Invesco QQQ Trust", Exchange = "NASDAQ", AssetType = "ETF", Currency = "USD", LastPrice = 380.25m },
                new() { Symbol = "VOO", Name = "Vanguard S&P 500 ETF", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 410.40m },
                new() { Symbol = "VTI", Name = "Vanguard Total Stock Market ETF", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 230.75m },
                new() { Symbol = "DIA", Name = "SPDR Dow Jones Industrial Average ETF", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 350.20m },
                new() { Symbol = "IWM", Name = "iShares Russell 2000 ETF", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 195.50m },
                new() { Symbol = "EEM", Name = "iShares MSCI Emerging Markets ETF", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 42.30m },
                new() { Symbol = "GLD", Name = "SPDR Gold Trust", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 185.60m },
            };

            // Filter by query (symbol or name contains the query string)
            var results = popularStocks
                .Where(s => s.Symbol.Contains(query) || s.Name.ToUpperInvariant().Contains(query))
                .Take(limit)
                .ToList();

            _logger.LogInformation("Found {Count} stocks matching query: {Query}", results.Count, query);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching stocks with query: {Query}", query);
            throw;
        }
    }

    /// <summary>
    /// Get detailed stock information / 获取详细股票信息
    /// </summary>
    public async Task<StockInfo?> GetStockInfoAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            symbol = NormalizeSymbol(symbol);
            _logger.LogInformation("Getting stock info for: {Symbol}", symbol);

            // TODO: Integrate with actual stock data source
            // For now, return mock data for common stocks

            var mockStockInfos = new Dictionary<string, StockInfo>
            {
                ["AAPL"] = new()
                {
                    Symbol = "AAPL",
                    Name = "Apple Inc.",
                    Exchange = "NASDAQ",
                    AssetType = "Stock",
                    Currency = "USD",
                    Sector = "Technology",
                    Industry = "Consumer Electronics",
                    MarketCap = 2800000000000m,
                    Description = "Apple Inc. designs, manufactures, and markets smartphones, personal computers, tablets, wearables, and accessories worldwide.",
                    CEO = "Tim Cook",
                    Employees = 164000,
                    Website = "https://www.apple.com",
                    LastPrice = 180.50m,
                    Week52High = 198.23m,
                    Week52Low = 164.08m,
                    AverageVolume = 58000000,
                    PERatio = 29.5m,
                    DividendYield = 0.48m
                },
                ["MSFT"] = new()
                {
                    Symbol = "MSFT",
                    Name = "Microsoft Corporation",
                    Exchange = "NASDAQ",
                    AssetType = "Stock",
                    Currency = "USD",
                    Sector = "Technology",
                    Industry = "Software",
                    MarketCap = 2750000000000m,
                    Description = "Microsoft Corporation develops, licenses, and supports software, services, devices, and solutions worldwide.",
                    CEO = "Satya Nadella",
                    Employees = 221000,
                    Website = "https://www.microsoft.com",
                    LastPrice = 370.25m,
                    Week52High = 384.30m,
                    Week52Low = 309.45m,
                    AverageVolume = 24000000,
                    PERatio = 34.2m,
                    DividendYield = 0.72m
                },
                ["SPY"] = new()
                {
                    Symbol = "SPY",
                    Name = "SPDR S&P 500 ETF Trust",
                    Exchange = "NYSE",
                    AssetType = "ETF",
                    Currency = "USD",
                    Description = "The SPDR S&P 500 ETF Trust seeks to provide investment results that correspond to the price and yield performance of the S&P 500 Index.",
                    LastPrice = 450.60m,
                    Week52High = 475.20m,
                    Week52Low = 395.80m,
                    AverageVolume = 78000000,
                    DividendYield = 1.45m
                },
            };

            if (mockStockInfos.TryGetValue(symbol, out var stockInfo))
            {
                return stockInfo;
            }

            // Return minimal info for other symbols
            return new StockInfo
            {
                Symbol = symbol,
                Name = $"{symbol} Inc.",
                Exchange = "NASDAQ",
                AssetType = "Stock",
                Currency = "USD"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock info for: {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Get list of popular/trending stocks / 获取热门股票列表
    /// </summary>
    public async Task<List<StockSearchResult>> GetPopularStocksAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting popular stocks, limit: {Limit}", limit);

            // TODO: Integrate with actual data source for trending stocks
            // For now, return a curated list of popular stocks and ETFs

            var popularStocks = new List<StockSearchResult>
            {
                new() { Symbol = "AAPL", Name = "Apple Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 180.50m },
                new() { Symbol = "MSFT", Name = "Microsoft Corporation", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 370.25m },
                new() { Symbol = "GOOGL", Name = "Alphabet Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 140.75m },
                new() { Symbol = "AMZN", Name = "Amazon.com Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 160.30m },
                new() { Symbol = "TSLA", Name = "Tesla Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 240.85m },
                new() { Symbol = "META", Name = "Meta Platforms Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 380.15m },
                new() { Symbol = "NVDA", Name = "NVIDIA Corporation", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD", LastPrice = 470.90m },
                new() { Symbol = "SPY", Name = "SPDR S&P 500 ETF Trust", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 450.60m },
                new() { Symbol = "QQQ", Name = "Invesco QQQ Trust", Exchange = "NASDAQ", AssetType = "ETF", Currency = "USD", LastPrice = 380.25m },
                new() { Symbol = "VOO", Name = "Vanguard S&P 500 ETF", Exchange = "NYSE", AssetType = "ETF", Currency = "USD", LastPrice = 410.40m },
            };

            return popularStocks.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular stocks");
            throw;
        }
    }

    #endregion
}
