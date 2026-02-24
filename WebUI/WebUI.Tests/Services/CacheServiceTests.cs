using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Core.Models;
using WebUI.Core.Services;
using Xunit;

namespace WebUI.Tests.Services;

/// <summary>
/// Unit tests for MemoryCacheService / 内存缓存服务单元测试
/// </summary>
public class MemoryCacheServiceTests : IDisposable
{
    private readonly Mock<ILogger<MemoryCacheService>> _mockLogger;
    private readonly MemoryCacheService _cacheService;

    public MemoryCacheServiceTests()
    {
        _mockLogger = new Mock<ILogger<MemoryCacheService>>();
        _cacheService = new MemoryCacheService(_mockLogger.Object);
    }

    [Fact]
    public async Task SetAsync_AndGetAsync_StoresAndRetrievesValue()
    {
        // Arrange
        var key = "test:key";
        var value = new MarketQuote
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
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<MarketQuote>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value.Symbol, result.Symbol);
        Assert.Equal(value.LastPrice, result.LastPrice);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var key = "nonexistent:key";

        // Act
        var result = await _cacheService.GetAsync<MarketQuote>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithExpiredKey_ReturnsNull()
    {
        // Arrange
        var key = "expired:key";
        var value = new MarketQuote
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
        await _cacheService.SetAsync(key, value, TimeSpan.FromMilliseconds(100));
        await Task.Delay(200); // Wait for expiration
        var result = await _cacheService.GetAsync<MarketQuote>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_RemovesValue()
    {
        // Arrange
        var key = "remove:key";
        var value = new MarketQuote
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
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<MarketQuote>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueForExistingKey()
    {
        // Arrange
        var key = "exists:key";
        var value = new MarketQuote
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
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalseForNonExistentKey()
    {
        // Arrange
        var key = "nonexistent:key";

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetManyAsync_RetrievesMultipleValues()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var values = new Dictionary<string, MarketQuote>
        {
            ["key1"] = new MarketQuote
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
            },
            ["key2"] = new MarketQuote
            {
                Symbol = "GOOGL",
                LastPrice = 2800.00m,
                PreviousClose = 2750.00m,
                Timestamp = DateTime.UtcNow,
                LastSize = 100,
                BidSize = 200,
                AskSize = 200,
                High = 2850,
                Low = 2740,
                Open = 2760,
                Volume = 500000,
                BidPrice = 2799,
                AskPrice = 2801,
                IsDelayed = false
            },
            ["key3"] = new MarketQuote
            {
                Symbol = "MSFT",
                LastPrice = 300.00m,
                PreviousClose = 298.00m,
                Timestamp = DateTime.UtcNow,
                LastSize = 100,
                BidSize = 300,
                AskSize = 300,
                High = 301,
                Low = 297,
                Open = 299,
                Volume = 800000,
                BidPrice = 299.95m,
                AskPrice = 300.05m,
                IsDelayed = false
            }
        };

        // Act
        await _cacheService.SetManyAsync(values, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetManyAsync<MarketQuote>(keys);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("AAPL", result["key1"].Symbol);
        Assert.Equal("GOOGL", result["key2"].Symbol);
        Assert.Equal("MSFT", result["key3"].Symbol);
    }

    [Fact]
    public async Task SetManyAsync_StoresMultipleValues()
    {
        // Arrange
        var values = new Dictionary<string, MarketQuote>
        {
            ["multi1"] = new MarketQuote
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
            },
            ["multi2"] = new MarketQuote
            {
                Symbol = "GOOGL",
                LastPrice = 2800.00m,
                PreviousClose = 2750.00m,
                Timestamp = DateTime.UtcNow,
                LastSize = 100,
                BidSize = 200,
                AskSize = 200,
                High = 2850,
                Low = 2740,
                Open = 2760,
                Volume = 500000,
                BidPrice = 2799,
                AskPrice = 2801,
                IsDelayed = false
            }
        };

        // Act
        await _cacheService.SetManyAsync(values, TimeSpan.FromMinutes(5));
        var result1 = await _cacheService.GetAsync<MarketQuote>("multi1");
        var result2 = await _cacheService.GetAsync<MarketQuote>("multi2");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal("AAPL", result1.Symbol);
        Assert.Equal("GOOGL", result2.Symbol);
    }

    public void Dispose()
    {
        _cacheService?.Dispose();
    }
}
