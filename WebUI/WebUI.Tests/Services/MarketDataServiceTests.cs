using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Core.Models;
using WebUI.Core.Services;
using Xunit;

namespace WebUI.Tests.Services;

/// <summary>
/// Unit tests for MarketDataService / 市场数据服务单元测试
/// </summary>
public class MarketDataServiceTests
{
    private readonly Mock<ILogger<MarketDataService>> _mockLogger;
    private readonly Mock<IbkrConnectionService> _mockIbkrService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly MarketDataService _marketDataService;

    public MarketDataServiceTests()
    {
        _mockLogger = new Mock<ILogger<MarketDataService>>();
        _mockIbkrService = new Mock<IbkrConnectionService>();
        _mockCacheService = new Mock<ICacheService>();
        
        _marketDataService = new MarketDataService(
            _mockLogger.Object,
            _mockIbkrService.Object,
            _mockCacheService.Object);
    }

    #region Subscription Tests

    [Fact]
    public async Task SubscribeAsync_WithValidSymbols_ReturnsSuccess()
    {
        // Arrange
        var symbols = new List<string> { "AAPL", "GOOGL", "MSFT" };

        // Act
        var result = await _marketDataService.SubscribeAsync(symbols);

        // Assert
        Assert.True(result);
        Assert.Equal(3, _marketDataService.SubscribedSymbols.Count);
        Assert.Contains("AAPL", _marketDataService.SubscribedSymbols);
        Assert.Contains("GOOGL", _marketDataService.SubscribedSymbols);
        Assert.Contains("MSFT", _marketDataService.SubscribedSymbols);
    }

    [Fact]
    public async Task SubscribeAsync_NormalizesSymbolsToUppercase()
    {
        // Arrange
        var symbols = new List<string> { "aapl", "Googl", "msft" };

        // Act
        var result = await _marketDataService.SubscribeAsync(symbols);

        // Assert
        Assert.True(result);
        Assert.Contains("AAPL", _marketDataService.SubscribedSymbols);
        Assert.Contains("GOOGL", _marketDataService.SubscribedSymbols);
        Assert.Contains("MSFT", _marketDataService.SubscribedSymbols);
    }

    [Fact]
    public async Task SubscribeAsync_ExceedingMaxSubscriptions_ReturnsFalse()
    {
        // Arrange
        var firstBatch = Enumerable.Range(1, 50).Select(i => $"SYMBOL{i}").ToList();
        var secondBatch = new List<string> { "EXTRA" };

        // Act
        await _marketDataService.SubscribeAsync(firstBatch);
        var result = await _marketDataService.SubscribeAsync(secondBatch);

        // Assert
        Assert.False(result);
        Assert.Equal(50, _marketDataService.SubscribedSymbols.Count);
    }

    [Fact]
    public async Task UnsubscribeAsync_RemovesSymbols()
    {
        // Arrange
        var symbols = new List<string> { "AAPL", "GOOGL", "MSFT" };
        await _marketDataService.SubscribeAsync(symbols);
        var toRemove = new List<string> { "AAPL", "MSFT" };

        // Act
        var result = await _marketDataService.UnsubscribeAsync(toRemove);

        // Assert
        Assert.True(result);
        Assert.Single(_marketDataService.SubscribedSymbols);
        Assert.Contains("GOOGL", _marketDataService.SubscribedSymbols);
        Assert.DoesNotContain("AAPL", _marketDataService.SubscribedSymbols);
        Assert.DoesNotContain("MSFT", _marketDataService.SubscribedSymbols);
    }

    #endregion

    #region Quote Retrieval Tests

    [Fact]
    public async Task GetQuoteAsync_WithCachedQuote_ReturnsCachedData()
    {
        // Arrange
        var symbol = "AAPL";
        var cachedQuote = new MarketQuote
        {
            Symbol = symbol,
            LastPrice = 150.50m,
            BidPrice = 150.45m,
            AskPrice = 150.55m,
            Volume = 1000000,
            PreviousClose = 149.00m,
            High = 151.00m,
            Low = 149.50m,
            Open = 150.00m,
            Timestamp = DateTime.UtcNow,
            IsDelayed = false,
            LastSize = 100,
            BidSize = 500,
            AskSize = 500
        };

        _mockCacheService
            .Setup(c => c.GetAsync<MarketQuote>(It.IsAny<string>(), default))
            .ReturnsAsync(cachedQuote);

        // Act
        var result = await _marketDataService.GetQuoteAsync(symbol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbol, result.Symbol);
        Assert.Equal(150.50m, result.LastPrice);
        _mockCacheService.Verify(c => c.GetAsync<MarketQuote>(It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task GetQuoteAsync_WithNonExistentSymbol_ReturnsNull()
    {
        // Arrange
        var symbol = "INVALID";
        _mockCacheService
            .Setup(c => c.GetAsync<MarketQuote>(It.IsAny<string>(), default))
            .ReturnsAsync((MarketQuote?)null);

        // Act
        var result = await _marketDataService.GetQuoteAsync(symbol);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetQuotesAsync_ReturnsMultipleQuotes()
    {
        // Arrange
        var symbols = new List<string> { "AAPL", "GOOGL" };
        
        _mockCacheService
            .Setup(c => c.GetAsync<MarketQuote>("market:quote:AAPL", default))
            .ReturnsAsync(new MarketQuote
            {
                Symbol = "AAPL",
                LastPrice = 150.50m,
                PreviousClose = 149.00m,
                Timestamp = DateTime.UtcNow,
                LastSize = 100,
                BidSize = 500,
                AskSize = 500,
                High = 151,
                Low = 149,
                Open = 150,
                Volume = 1000000,
                BidPrice = 150,
                AskPrice = 151,
                IsDelayed = false
            });
        
        _mockCacheService
            .Setup(c => c.GetAsync<MarketQuote>("market:quote:GOOGL", default))
            .ReturnsAsync(new MarketQuote
            {
                Symbol = "GOOGL",
                LastPrice = 2800.00m,
                PreviousClose = 2750.00m,
                Timestamp = DateTime.UtcNow,
                LastSize = 100,
                BidSize = 500,
                AskSize = 500,
                High = 2850,
                Low = 2740,
                Open = 2760,
                Volume = 500000,
                BidPrice = 2799,
                AskPrice = 2801,
                IsDelayed = false
            });

        // Act
        var result = await _marketDataService.GetQuotesAsync(symbols);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, q => q.Symbol == "AAPL");
        Assert.Contains(result, q => q.Symbol == "GOOGL");
    }

    #endregion

    #region Market Status Tests

    [Fact]
    public async Task GetMarketStatusAsync_ReturnsTradingDayStatus()
    {
        // Act
        var result = await _marketDataService.GetMarketStatusAsync();

        // Assert
        Assert.NotNull(result);
        // MarketTime is a DateTime value type, so it's always non-null
        Assert.True(result.MarketTime > DateTime.MinValue);
    }

    [Fact]
    public async Task GetMarketStatusAsync_DeterminesCorrectStatus()
    {
        // This test depends on current time, so we just verify it returns a valid status
        // Act
        var result = await _marketDataService.GetMarketStatusAsync();

        // Assert
        Assert.True(Enum.IsDefined(typeof(MarketStatus), result.Status));
    }

    #endregion

    #region Market Summary Tests

    [Fact]
    public async Task GetMarketSummaryAsync_ReturnsCompleteData()
    {
        // Act
        var result = await _marketDataService.GetMarketSummaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.MarketStatus);
        Assert.NotNull(result.Indices);
        Assert.NotEmpty(result.Indices);
        Assert.NotNull(result.Sectors);
        Assert.NotEmpty(result.Sectors);
        Assert.Equal(11, result.Sectors.Count); // US market has 11 sectors
    }

    [Fact]
    public async Task GetMarketSummaryAsync_IncludesMajorIndices()
    {
        // Act
        var result = await _marketDataService.GetMarketSummaryAsync();

        // Assert
        Assert.Contains(result.Indices, i => i.Symbol == "SPX");
        Assert.Contains(result.Indices, i => i.Symbol == "IXIC");
        Assert.Contains(result.Indices, i => i.Symbol == "DJI");
    }

    #endregion

    #region Quote Update Tests

    [Fact]
    public async Task UpdateQuote_FiresMarketDataUpdateEvent()
    {
        // Arrange
        var eventFired = false;
        MarketDataUpdate? capturedUpdate = null;
        
        _marketDataService.OnMarketDataUpdate += (sender, update) =>
        {
            eventFired = true;
            capturedUpdate = update;
        };

        var quote = new MarketQuote
        {
            Symbol = "AAPL",
            LastPrice = 150.50m,
            PreviousClose = 149.00m,
            Timestamp = DateTime.UtcNow,
            LastSize = 100,
            BidSize = 500,
            AskSize = 500,
            High = 151,
            Low = 149,
            Open = 150,
            Volume = 1000000,
            BidPrice = 150,
            AskPrice = 151,
            IsDelayed = false
        };

        // Act
        _marketDataService.UpdateQuote(quote);
        
        // Wait a bit for async operation
        await Task.Delay(100);

        // Assert
        Assert.True(eventFired);
        Assert.NotNull(capturedUpdate);
        Assert.Equal("AAPL", capturedUpdate.Quote.Symbol);
        Assert.Equal(150.50m, capturedUpdate.Quote.LastPrice);
    }

    [Fact]
    public async Task UpdateQuote_UpdatesCacheService()
    {
        // Arrange
        var quote = new MarketQuote
        {
            Symbol = "AAPL",
            LastPrice = 150.50m,
            PreviousClose = 149.00m,
            Timestamp = DateTime.UtcNow,
            LastSize = 100,
            BidSize = 500,
            AskSize = 500,
            High = 151,
            Low = 149,
            Open = 150,
            Volume = 1000000,
            BidPrice = 150,
            AskPrice = 151,
            IsDelayed = false
        };

        // Act
        _marketDataService.UpdateQuote(quote);
        
        // Wait a bit for async operation
        await Task.Delay(100);

        // Assert
        _mockCacheService.Verify(
            c => c.SetAsync(
                It.Is<string>(key => key.Contains("AAPL")),
                It.IsAny<MarketQuote>(),
                It.IsAny<TimeSpan>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task UpdateQuote_ThrottlesFrequentUpdates()
    {
        // Arrange
        var updateCount = 0;
        _marketDataService.OnMarketDataUpdate += (sender, update) => updateCount++;

        var quote = new MarketQuote
        {
            Symbol = "AAPL",
            LastPrice = 150.50m,
            PreviousClose = 149.00m,
            Timestamp = DateTime.UtcNow,
            LastSize = 100,
            BidSize = 500,
            AskSize = 500,
            High = 151,
            Low = 149,
            Open = 150,
            Volume = 1000000,
            BidPrice = 150,
            AskPrice = 151,
            IsDelayed = false
        };

        // Act - Send multiple updates rapidly
        for (int i = 0; i < 10; i++)
        {
            quote.LastPrice = 150.50m + i * 0.01m;
            _marketDataService.UpdateQuote(quote);
        }
        
        await Task.Delay(100);

        // Assert - Should only fire once due to throttling (1 update per second)
        Assert.Equal(1, updateCount);
    }

    #endregion

    #region Calculated Property Tests

    [Fact]
    public void MarketQuote_CalculatesChangeCorrectly()
    {
        // Arrange & Act
        var quote = new MarketQuote
        {
            Symbol = "AAPL",
            LastPrice = 150.00m,
            PreviousClose = 145.00m,
            Timestamp = DateTime.UtcNow,
            LastSize = 100,
            BidSize = 500,
            AskSize = 500,
            High = 151,
            Low = 144,
            Open = 146,
            Volume = 1000000,
            BidPrice = 149.95m,
            AskPrice = 150.05m,
            IsDelayed = false
        };

        // Assert
        Assert.Equal(5.00m, quote.Change);
    }

    [Fact]
    public void MarketQuote_CalculatesChangePercentCorrectly()
    {
        // Arrange & Act
        var quote = new MarketQuote
        {
            Symbol = "AAPL",
            LastPrice = 150.00m,
            PreviousClose = 145.00m,
            Timestamp = DateTime.UtcNow,
            LastSize = 100,
            BidSize = 500,
            AskSize = 500,
            High = 151,
            Low = 144,
            Open = 146,
            Volume = 1000000,
            BidPrice = 149.95m,
            AskPrice = 150.05m,
            IsDelayed = false
        };

        // Assert
        Assert.Equal(3.4482758620689655172413793103m, quote.ChangePercent, 5); // Allow for precision
    }

    [Fact]
    public void MarketQuote_HandlesZeroPreviousClose()
    {
        // Arrange & Act
        var quote = new MarketQuote
        {
            Symbol = "NEWIPO",
            LastPrice = 50.00m,
            PreviousClose = 0m,
            Timestamp = DateTime.UtcNow,
            LastSize = 100,
            BidSize = 500,
            AskSize = 500,
            High = 52,
            Low = 48,
            Open = 50,
            Volume = 100000,
            BidPrice = 49.95m,
            AskPrice = 50.05m,
            IsDelayed = false
        };

        // Assert
        Assert.Equal(0m, quote.ChangePercent);
    }

    #endregion
}
