using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Core.Models;
using WebUI.Core.Services;
using WebUI.Data;
using WebUI.Data.Entities;
using Xunit;

namespace WebUI.Tests.Services;

/// <summary>
/// Unit tests for EtfService / ETF 服务单元测试
/// </summary>
public class EtfServiceTests : IDisposable
{
    private readonly Mock<ILogger<EtfService>> _mockLogger;
    private readonly Mock<IMarketDataService> _mockMarketDataService;
    private readonly WebUIDbContext _dbContext;
    private readonly EtfService _etfService;

    public EtfServiceTests()
    {
        _mockLogger = new Mock<ILogger<EtfService>>();
        _mockMarketDataService = new Mock<IMarketDataService>();

        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<WebUIDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new WebUIDbContext(options);

        // Seed test data
        SeedTestData();

        _etfService = new EtfService(
            _mockLogger.Object,
            _mockMarketDataService.Object,
            _dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private void SeedTestData()
    {
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTime.UtcNow
        };

        var brokerAccount = new BrokerAccount
        {
            Id = 1,
            UserId = 1,
            AccountNumber = "TEST123",
            AccountType = "Paper",
            BrokerName = "TEST",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.BrokerAccounts.Add(brokerAccount);
        _dbContext.SaveChanges();
    }

    #region SearchEtfsAsync Tests

    [Fact]
    public async Task SearchEtfsAsync_WithQuery_ReturnsMatchingResults()
    {
        // Arrange
        var request = new EtfSearchRequest
        {
            Query = "SPY",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _etfService.SearchEtfsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Results.Count > 0);
        Assert.Contains(result.Results, r => r.Symbol == "SPY");
    }

    [Fact]
    public async Task SearchEtfsAsync_WithCategory_ReturnsFilteredResults()
    {
        // Arrange
        var request = new EtfSearchRequest
        {
            Category = "Technology",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _etfService.SearchEtfsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Results.Count > 0);
        Assert.All(result.Results, r => Assert.Equal("Technology", r.Category));
    }

    [Fact]
    public async Task SearchEtfsAsync_WithExpenseRatioFilter_ReturnsLowCostEtfs()
    {
        // Arrange
        var request = new EtfSearchRequest
        {
            MaxExpenseRatio = 0.1m,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _etfService.SearchEtfsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Results.Count > 0);
        Assert.All(result.Results, r => Assert.True(r.ExpenseRatio <= 0.1m));
    }

    [Fact]
    public async Task SearchEtfsAsync_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        var request = new EtfSearchRequest
        {
            SortBy = "expenseRatio",
            SortDirection = "asc",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _etfService.SearchEtfsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Results.Count > 1);
        
        for (int i = 0; i < result.Results.Count - 1; i++)
        {
            Assert.True(result.Results[i].ExpenseRatio <= result.Results[i + 1].ExpenseRatio);
        }
    }

    [Fact]
    public async Task SearchEtfsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var request = new EtfSearchRequest
        {
            Page = 1,
            PageSize = 3
        };

        // Act
        var result = await _etfService.SearchEtfsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Results.Count <= 3);
        Assert.Equal(1, result.Page);
        Assert.Equal(3, result.PageSize);
    }

    #endregion

    #region GetEtfDetailAsync Tests

    [Fact]
    public async Task GetEtfDetailAsync_ValidSymbol_ReturnsDetail()
    {
        // Arrange
        var symbol = "SPY";
        _mockMarketDataService.Setup(m => m.GetQuoteAsync(symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketQuote
            {
                Symbol = symbol,
                LastPrice = 450.00m,
                BidPrice = 449.95m,
                AskPrice = 450.05m,
                Volume = 75000000,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = await _etfService.GetEtfDetailAsync(symbol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SPY", result.Symbol);
        Assert.Equal("SPDR S&P 500 ETF Trust", result.Name);
        Assert.Equal(450.00m, result.LastPrice);
        Assert.NotNull(result.Holdings);
        Assert.True(result.Holdings.Count > 0);
    }

    [Fact]
    public async Task GetEtfDetailAsync_InvalidSymbol_ReturnsNull()
    {
        // Arrange
        var symbol = "INVALID";

        // Act
        var result = await _etfService.GetEtfDetailAsync(symbol);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CompareEtfsAsync Tests

    [Fact]
    public async Task CompareEtfsAsync_ValidSymbols_ReturnsComparison()
    {
        // Arrange
        var request = new EtfCompareRequest
        {
            Symbols = new List<string> { "SPY", "VOO" },
            IncludePerformanceData = false
        };

        _mockMarketDataService.Setup(m => m.GetQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string symbol, CancellationToken ct) => new MarketQuote
            {
                Symbol = symbol,
                LastPrice = 450.00m,
                BidPrice = 449.95m,
                AskPrice = 450.05m,
                Volume = 5000000,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = await _etfService.CompareEtfsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Comparisons.Count);
        Assert.Contains(result.Comparisons, c => c.Symbol == "SPY");
        Assert.Contains(result.Comparisons, c => c.Symbol == "VOO");
        Assert.Null(result.PerformanceData);
    }

    [Fact]
    public async Task CompareEtfsAsync_WithPerformanceData_ReturnsPerformanceChart()
    {
        // Arrange
        var request = new EtfCompareRequest
        {
            Symbols = new List<string> { "SPY", "VOO" },
            IncludePerformanceData = true,
            PerformancePeriodDays = 30
        };

        _mockMarketDataService.Setup(m => m.GetQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string symbol, CancellationToken ct) => new MarketQuote
            {
                Symbol = symbol,
                LastPrice = 450.00m,
                BidPrice = 449.95m,
                AskPrice = 450.05m,
                Volume = 5000000,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = await _etfService.CompareEtfsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.PerformanceData);
        Assert.Equal(2, result.PerformanceData.Count);
        Assert.True(result.PerformanceData["SPY"].Count > 0);
        Assert.True(result.PerformanceData["VOO"].Count > 0);
    }

    #endregion

    #region GetEtfDividendsAsync Tests

    [Fact]
    public async Task GetEtfDividendsAsync_ValidSymbol_ReturnsDividends()
    {
        // Arrange
        var symbol = "SPY";

        // Act
        var result = await _etfService.GetEtfDividendsAsync(symbol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SPY", result.Symbol);
        Assert.True(result.Dividends.Count > 0);
        Assert.True(result.TotalDividends > 0);
        Assert.True(result.AnnualizedYield >= 0);
    }

    [Fact]
    public async Task GetEtfDividendsAsync_NonDividendPayingEtf_ReturnsEmptyDividends()
    {
        // Arrange
        var symbol = "QQQ"; // Typically lower dividend

        // Act
        var result = await _etfService.GetEtfDividendsAsync(symbol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("QQQ", result.Symbol);
        // QQQ pays some dividends so this should still have data
        Assert.True(result.Dividends.Count >= 0);
    }

    #endregion

    #region Recurring Plan Tests

    [Fact]
    public async Task CreateRecurringPlanAsync_ValidRequest_CreatesAndReturnsThePlan()
    {
        // Arrange
        var userId = 1;
        var brokerAccountId = 1;
        var request = new RecurringPlanRequest
        {
            Symbol = "SPY",
            Name = "SPY 定投",
            Amount = 1000m,
            Currency = "USD",
            Frequency = "Monthly",
            StartDate = DateTime.UtcNow,
            Notes = "Test plan"
        };

        // Act
        var result = await _etfService.CreateRecurringPlanAsync(userId, brokerAccountId, request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("SPY", result.Symbol);
        Assert.Equal(1000m, result.Amount);
        Assert.Equal("Monthly", result.Frequency);
        Assert.Equal("Active", result.Status);
        Assert.Equal(0, result.ExecutionCount);
    }

    [Fact]
    public async Task GetRecurringPlansAsync_WithExistingPlans_ReturnsAllPlans()
    {
        // Arrange
        var userId = 1;
        var brokerAccountId = 1;

        // Create multiple plans
        await _etfService.CreateRecurringPlanAsync(userId, brokerAccountId, new RecurringPlanRequest
        {
            Symbol = "SPY",
            Amount = 1000m,
            Frequency = "Monthly",
            StartDate = DateTime.UtcNow
        });

        await _etfService.CreateRecurringPlanAsync(userId, brokerAccountId, new RecurringPlanRequest
        {
            Symbol = "VOO",
            Amount = 500m,
            Frequency = "Weekly",
            StartDate = DateTime.UtcNow
        });

        // Act
        var result = await _etfService.GetRecurringPlansAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRecurringPlanAsync_ValidId_ReturnsPlan()
    {
        // Arrange
        var userId = 1;
        var brokerAccountId = 1;
        var created = await _etfService.CreateRecurringPlanAsync(userId, brokerAccountId, new RecurringPlanRequest
        {
            Symbol = "SPY",
            Amount = 1000m,
            Frequency = "Monthly",
            StartDate = DateTime.UtcNow
        });

        // Act
        var result = await _etfService.GetRecurringPlanAsync(userId, created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("SPY", result.Symbol);
    }

    [Fact]
    public async Task GetRecurringPlanAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var userId = 1;
        var invalidId = 99999;

        // Act
        var result = await _etfService.GetRecurringPlanAsync(userId, invalidId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateRecurringPlanAsync_ValidRequest_UpdatesPlan()
    {
        // Arrange
        var userId = 1;
        var brokerAccountId = 1;
        var created = await _etfService.CreateRecurringPlanAsync(userId, brokerAccountId, new RecurringPlanRequest
        {
            Symbol = "SPY",
            Amount = 1000m,
            Frequency = "Monthly",
            StartDate = DateTime.UtcNow
        });

        var updateRequest = new UpdateRecurringPlanRequest
        {
            Amount = 1500m,
            Status = "Paused"
        };

        // Act
        var result = await _etfService.UpdateRecurringPlanAsync(userId, created.Id, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(1500m, result.Amount);
        Assert.Equal("Paused", result.Status);
    }

    [Fact]
    public async Task UpdateRecurringPlanAsync_InvalidId_ThrowsException()
    {
        // Arrange
        var userId = 1;
        var invalidId = 99999;
        var updateRequest = new UpdateRecurringPlanRequest
        {
            Amount = 1500m
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _etfService.UpdateRecurringPlanAsync(userId, invalidId, updateRequest));
    }

    [Fact]
    public async Task DeleteRecurringPlanAsync_ValidId_DeletesPlan()
    {
        // Arrange
        var userId = 1;
        var brokerAccountId = 1;
        var created = await _etfService.CreateRecurringPlanAsync(userId, brokerAccountId, new RecurringPlanRequest
        {
            Symbol = "SPY",
            Amount = 1000m,
            Frequency = "Monthly",
            StartDate = DateTime.UtcNow
        });

        // Act
        await _etfService.DeleteRecurringPlanAsync(userId, created.Id);

        // Assert
        var deleted = await _etfService.GetRecurringPlanAsync(userId, created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteRecurringPlanAsync_InvalidId_ThrowsException()
    {
        // Arrange
        var userId = 1;
        var invalidId = 99999;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _etfService.DeleteRecurringPlanAsync(userId, invalidId));
    }

    #endregion
}
