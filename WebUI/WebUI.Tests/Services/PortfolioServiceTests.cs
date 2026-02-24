using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Core.Models;
using WebUI.Core.Services;
using WebUI.Data;
using WebUI.Data.Entities;
using WebUI.Data.Repositories;
using Xunit;

namespace WebUI.Tests.Services;

/// <summary>
/// Unit tests for PortfolioService / 投资组合服务单元测试
/// </summary>
public class PortfolioServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<PortfolioService>> _mockLogger;
    private readonly Mock<IMarketDataService> _mockMarketDataService;
    private readonly Mock<IPositionRepository> _mockPositionRepo;
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<IRepository<BrokerAccount>> _mockAccountRepo;
    private readonly PortfolioService _portfolioService;

    public PortfolioServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<PortfolioService>>();
        _mockMarketDataService = new Mock<IMarketDataService>();
        _mockPositionRepo = new Mock<IPositionRepository>();
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockAccountRepo = new Mock<IRepository<BrokerAccount>>();

        // Setup UnitOfWork to return repositories
        _mockUnitOfWork.Setup(u => u.GetRepository<Position>()).Returns(_mockPositionRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Order>()).Returns(_mockOrderRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<BrokerAccount>()).Returns(_mockAccountRepo.Object);
        _mockUnitOfWork.Setup(u => u.Positions).Returns(_mockPositionRepo.Object);

        _portfolioService = new PortfolioService(
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockMarketDataService.Object);
    }

    #region GetPositionsAsync Tests

    [Fact]
    public async Task GetPositionsAsync_WithPositions_ReturnsPortfolioResponse()
    {
        // Arrange
        var brokerAccountId = 1;
        var positions = new List<Position>
        {
            new Position
            {
                Id = 1,
                BrokerAccountId = brokerAccountId,
                Symbol = "AAPL",
                Quantity = 100,
                AverageCost = 150.00m,
                CurrentPrice = 160.00m,
                UnrealizedPnL = 1000.00m,
                RealizedPnL = 0,
                FirstPurchaseDate = DateTime.UtcNow.AddDays(-30),
                LastUpdatedAt = DateTime.UtcNow
            },
            new Position
            {
                Id = 2,
                BrokerAccountId = brokerAccountId,
                Symbol = "GOOGL",
                Quantity = 50,
                AverageCost = 2800.00m,
                CurrentPrice = 2900.00m,
                UnrealizedPnL = 5000.00m,
                RealizedPnL = 0,
                FirstPurchaseDate = DateTime.UtcNow.AddDays(-60),
                LastUpdatedAt = DateTime.UtcNow
            }
        };

        var account = new BrokerAccount
        {
            Id = brokerAccountId,
            CashBalance = 50000m
        };

        _mockPositionRepo.Setup(r => r.GetByBrokerAccountIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);
        _mockAccountRepo.Setup(r => r.GetByIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Setup market data service
        _mockMarketDataService.Setup(m => m.GetQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string symbol, CancellationToken ct) => new MarketQuote
            {
                Symbol = symbol,
                LastPrice = symbol == "AAPL" ? 160.00m : 2900.00m
            });

        // Act
        var result = await _portfolioService.GetPositionsAsync(brokerAccountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Positions.Count);
        Assert.Equal(50000m, result.CashBalance);
        Assert.Equal(16000m + 145000m, result.TotalMarketValue); // AAPL: 100*160 + GOOGL: 50*2900
        Assert.Equal(6000m, result.TotalUnrealizedPnL); // 1000 + 5000
    }

    [Fact]
    public async Task GetPositionsAsync_WithNoPositions_ReturnsEmptyResponse()
    {
        // Arrange
        var brokerAccountId = 1;
        _mockPositionRepo.Setup(r => r.GetByBrokerAccountIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Position>());
        _mockAccountRepo.Setup(r => r.GetByIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrokerAccount { Id = brokerAccountId, CashBalance = 100000m });

        // Act
        var result = await _portfolioService.GetPositionsAsync(brokerAccountId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Positions);
        Assert.Equal(100000m, result.CashBalance);
        Assert.Equal(0, result.TotalMarketValue);
    }

    #endregion

    #region GetFilteredPositionsAsync Tests

    [Fact]
    public async Task GetFilteredPositionsAsync_WithProfitableFilter_ReturnsOnlyProfitablePositions()
    {
        // Arrange
        var brokerAccountId = 1;
        var positions = new List<Position>
        {
            new Position
            {
                Id = 1,
                Symbol = "AAPL",
                Quantity = 100,
                AverageCost = 150.00m,
                CurrentPrice = 160.00m,
                UnrealizedPnL = 1000.00m
            }
        };

        var options = new PositionQueryOptions
        {
            OnlyProfitable = true
        };

        _mockPositionRepo.Setup(r => r.GetFilteredPositionsAsync(
                brokerAccountId,
                null,
                true,
                null,
                "Symbol",
                "asc",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        _mockAccountRepo.Setup(r => r.GetByIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrokerAccount { Id = brokerAccountId, CashBalance = 50000m });

        _mockMarketDataService.Setup(m => m.GetQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketQuote { Symbol = "AAPL", LastPrice = 160.00m });

        // Act
        var result = await _portfolioService.GetFilteredPositionsAsync(brokerAccountId, options);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Positions);
        Assert.True(result.Positions[0].UnrealizedPnL > 0);
    }

    [Fact]
    public async Task GetFilteredPositionsAsync_WithSymbolFilter_ReturnsMatchingPositions()
    {
        // Arrange
        var brokerAccountId = 1;
        var positions = new List<Position>
        {
            new Position
            {
                Id = 1,
                Symbol = "AAPL",
                Quantity = 100,
                AverageCost = 150.00m,
                CurrentPrice = 160.00m,
                UnrealizedPnL = 1000.00m
            }
        };

        var options = new PositionQueryOptions
        {
            SymbolFilter = "AAPL"
        };

        _mockPositionRepo.Setup(r => r.GetFilteredPositionsAsync(
                brokerAccountId,
                "AAPL",
                null,
                null,
                "Symbol",
                "asc",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        _mockAccountRepo.Setup(r => r.GetByIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrokerAccount { Id = brokerAccountId, CashBalance = 50000m });

        _mockMarketDataService.Setup(m => m.GetQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketQuote { Symbol = "AAPL", LastPrice = 160.00m });

        // Act
        var result = await _portfolioService.GetFilteredPositionsAsync(brokerAccountId, options);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Positions);
        Assert.Equal("AAPL", result.Positions[0].Symbol);
    }

    #endregion

    #region GetPositionDetailAsync Tests

    [Fact]
    public async Task GetPositionDetailAsync_WithExistingPosition_ReturnsDetail()
    {
        // Arrange
        var brokerAccountId = 1;
        var symbol = "AAPL";
        var position = new Position
        {
            Id = 1,
            BrokerAccountId = brokerAccountId,
            Symbol = symbol,
            Quantity = 100,
            AverageCost = 150.00m,
            CurrentPrice = 160.00m,
            UnrealizedPnL = 1000.00m,
            RealizedPnL = 500.00m,
            FirstPurchaseDate = DateTime.UtcNow.AddDays(-30),
            LastUpdatedAt = DateTime.UtcNow
        };

        _mockPositionRepo.Setup(r => r.GetBySymbolAsync(brokerAccountId, symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        _mockMarketDataService.Setup(m => m.GetQuoteAsync(symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketQuote { Symbol = symbol, LastPrice = 160.00m });

        _mockOrderRepo.Setup(r => r.GetOrdersBySymbolAsync(brokerAccountId, symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        // Act
        var result = await _portfolioService.GetPositionDetailAsync(brokerAccountId, symbol);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbol, result.Symbol);
        Assert.Equal(100, result.Quantity);
        Assert.Equal(1000.00m, result.UnrealizedPnL);
        Assert.Equal(500.00m, result.RealizedPnL);
    }

    [Fact]
    public async Task GetPositionDetailAsync_WithNonExistentPosition_ReturnsNull()
    {
        // Arrange
        var brokerAccountId = 1;
        var symbol = "AAPL";

        _mockPositionRepo.Setup(r => r.GetBySymbolAsync(brokerAccountId, symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        // Act
        var result = await _portfolioService.GetPositionDetailAsync(brokerAccountId, symbol);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region ClosePositionAsync Tests

    [Fact]
    public async Task ClosePositionAsync_WithValidRequest_CreatesOrder()
    {
        // Arrange
        var brokerAccountId = 1;
        var symbol = "AAPL";
        var position = new Position
        {
            Id = 1,
            BrokerAccountId = brokerAccountId,
            Symbol = symbol,
            Quantity = 100,
            AverageCost = 150.00m,
            CurrentPrice = 160.00m
        };

        var request = new ClosePositionRequest
        {
            OrderType = "Market"
        };

        _mockPositionRepo.Setup(r => r.GetBySymbolAsync(brokerAccountId, symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        _mockOrderRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _portfolioService.ClosePositionAsync(brokerAccountId, symbol, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(symbol, result.Symbol);
        Assert.Equal("Sell", result.Side);
        Assert.Equal(100, result.Quantity);
        _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClosePositionAsync_WithPartialQuantity_CreatesCorrectOrder()
    {
        // Arrange
        var brokerAccountId = 1;
        var symbol = "AAPL";
        var position = new Position
        {
            Id = 1,
            BrokerAccountId = brokerAccountId,
            Symbol = symbol,
            Quantity = 100,
            AverageCost = 150.00m,
            CurrentPrice = 160.00m
        };

        var request = new ClosePositionRequest
        {
            Quantity = 50,
            OrderType = "Market"
        };

        _mockPositionRepo.Setup(r => r.GetBySymbolAsync(brokerAccountId, symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        _mockOrderRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _portfolioService.ClosePositionAsync(brokerAccountId, symbol, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Quantity);
    }

    [Fact]
    public async Task ClosePositionAsync_WithNonExistentPosition_ThrowsException()
    {
        // Arrange
        var brokerAccountId = 1;
        var symbol = "AAPL";
        var request = new ClosePositionRequest();

        _mockPositionRepo.Setup(r => r.GetBySymbolAsync(brokerAccountId, symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _portfolioService.ClosePositionAsync(brokerAccountId, symbol, request));
    }

    [Fact]
    public async Task ClosePositionAsync_WithExcessiveQuantity_ThrowsException()
    {
        // Arrange
        var brokerAccountId = 1;
        var symbol = "AAPL";
        var position = new Position
        {
            Id = 1,
            BrokerAccountId = brokerAccountId,
            Symbol = symbol,
            Quantity = 100
        };

        var request = new ClosePositionRequest
        {
            Quantity = 200
        };

        _mockPositionRepo.Setup(r => r.GetBySymbolAsync(brokerAccountId, symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _portfolioService.ClosePositionAsync(brokerAccountId, symbol, request));
    }

    #endregion

    #region GetPortfolioAllocationAsync Tests

    [Fact]
    public async Task GetPortfolioAllocationAsync_CalculatesCorrectPercentages()
    {
        // Arrange
        var brokerAccountId = 1;
        var positions = new List<Position>
        {
            new Position
            {
                Id = 1,
                Symbol = "AAPL",
                Quantity = 100,
                CurrentPrice = 160.00m, // 16,000
                UnrealizedPnL = 1000.00m
            },
            new Position
            {
                Id = 2,
                Symbol = "GOOGL",
                Quantity = 50,
                CurrentPrice = 2800.00m, // 140,000
                UnrealizedPnL = 5000.00m
            }
        };

        var account = new BrokerAccount
        {
            Id = brokerAccountId,
            CashBalance = 44000m // Total: 200,000
        };

        _mockPositionRepo.Setup(r => r.GetByBrokerAccountIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);
        _mockAccountRepo.Setup(r => r.GetByIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _mockMarketDataService.Setup(m => m.GetQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string symbol, CancellationToken ct) => new MarketQuote
            {
                Symbol = symbol,
                LastPrice = symbol == "AAPL" ? 160.00m : 2800.00m
            });

        // Act
        var result = await _portfolioService.GetPortfolioAllocationAsync(brokerAccountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200000m, result.TotalValue);
        Assert.Equal(2, result.Positions.Count);
        
        // AAPL should be 8% (16,000 / 200,000)
        var appleAllocation = result.Positions.First(p => p.Name == "AAPL");
        Assert.Equal(8m, appleAllocation.Percentage, 1);
        
        // GOOGL should be 70% (140,000 / 200,000)
        var googleAllocation = result.Positions.First(p => p.Name == "GOOGL");
        Assert.Equal(70m, googleAllocation.Percentage, 1);
        
        // Cash should be 22% (44,000 / 200,000)
        Assert.Equal(22m, result.Cash.Percentage, 1);
    }

    #endregion

    #region ExportPositionsToCsvAsync Tests

    [Fact]
    public async Task ExportPositionsToCsvAsync_GeneratesValidCsv()
    {
        // Arrange
        var brokerAccountId = 1;
        var positions = new List<Position>
        {
            new Position
            {
                Id = 1,
                Symbol = "AAPL",
                Quantity = 100,
                AverageCost = 150.00m,
                CurrentPrice = 160.00m,
                UnrealizedPnL = 1000.00m,
                RealizedPnL = 0,
                FirstPurchaseDate = DateTime.UtcNow.AddDays(-30)
            }
        };

        _mockPositionRepo.Setup(r => r.GetByBrokerAccountIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);
        _mockAccountRepo.Setup(r => r.GetByIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrokerAccount { Id = brokerAccountId, CashBalance = 50000m });

        _mockMarketDataService.Setup(m => m.GetQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketQuote { Symbol = "AAPL", LastPrice = 160.00m });

        // Act
        var result = await _portfolioService.ExportPositionsToCsvAsync(brokerAccountId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        var csv = System.Text.Encoding.UTF8.GetString(result);
        Assert.Contains("Symbol", csv);
        Assert.Contains("AAPL", csv);
        Assert.Contains("100", csv);
    }

    #endregion

    #region UpdatePositionPricesAsync Tests

    [Fact]
    public async Task UpdatePositionPricesAsync_UpdatesAllPositions()
    {
        // Arrange
        var brokerAccountId = 1;
        var positions = new List<Position>
        {
            new Position
            {
                Id = 1,
                Symbol = "AAPL",
                Quantity = 100,
                AverageCost = 150.00m,
                CurrentPrice = 150.00m
            },
            new Position
            {
                Id = 2,
                Symbol = "GOOGL",
                Quantity = 50,
                AverageCost = 2800.00m,
                CurrentPrice = 2800.00m
            }
        };

        _mockPositionRepo.Setup(r => r.GetByBrokerAccountIdAsync(brokerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        _mockMarketDataService.Setup(m => m.GetQuoteAsync("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketQuote { Symbol = "AAPL", LastPrice = 160.00m });
        _mockMarketDataService.Setup(m => m.GetQuoteAsync("GOOGL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketQuote { Symbol = "GOOGL", LastPrice = 2900.00m });

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _portfolioService.UpdatePositionPricesAsync(brokerAccountId);

        // Assert
        _mockMarketDataService.Verify(m => m.GetQuoteAsync("AAPL", It.IsAny<CancellationToken>()), Times.Once);
        _mockMarketDataService.Verify(m => m.GetQuoteAsync("GOOGL", It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
