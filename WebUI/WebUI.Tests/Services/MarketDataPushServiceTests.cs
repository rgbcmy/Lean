using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.API.Hubs;
using WebUI.API.Services;
using WebUI.Core.Services;

namespace WebUI.Tests.Services;

/// <summary>
/// Unit tests for MarketDataPushService.
/// MarketDataPushService 单元测试。
/// </summary>
public class MarketDataPushServiceTests
{
    private readonly Mock<IHubContext<MarketDataHub>> _mockHubContext;
    private readonly Mock<ILogger<MarketDataPushService>> _mockLogger;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;

    public MarketDataPushServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<MarketDataHub>>();
        _mockLogger = new Mock<ILogger<MarketDataPushService>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
    }

    [Fact]
    public async Task PushMarketDataAsync_WithValidData_SendsToGroup()
    {
        // Arrange
        var service = new MarketDataPushService(_mockHubContext.Object, _mockLogger.Object);
        var update = new MarketDataUpdate
        {
            Symbol = "AAPL",
            LastPrice = 150.50m,
            BidPrice = 150.49m,
            AskPrice = 150.51m,
            Volume = 1000000,
            Change = 2.50m,
            ChangePercent = 1.69m,
            MarketStatus = "Open",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await service.PushMarketDataAsync("AAPL", update);

        // Assert
        _mockClients.Verify(c => c.Group("MarketData:AAPL"), Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("MarketDataUpdate", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task PushMarketDataAsync_WithEmptySymbol_DoesNotSend()
    {
        // Arrange
        var service = new MarketDataPushService(_mockHubContext.Object, _mockLogger.Object);
        var update = new MarketDataUpdate { Symbol = "" };

        // Act
        await service.PushMarketDataAsync("", update);

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
            Times.Never
        );
    }

    [Fact]
    public async Task PushMarketDataAsync_ThrottlesFrequentUpdates()
    {
        // Arrange
        var service = new MarketDataPushService(_mockHubContext.Object, _mockLogger.Object);
        var update1 = new MarketDataUpdate { Symbol = "AAPL", LastPrice = 150.50m, Timestamp = DateTime.UtcNow };
        var update2 = new MarketDataUpdate { Symbol = "AAPL", LastPrice = 150.51m, Timestamp = DateTime.UtcNow };

        // Act - Push twice in quick succession
        await service.PushMarketDataAsync("AAPL", update1);
        await service.PushMarketDataAsync("AAPL", update2);

        // Assert - Should only send once due to throttling
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("MarketDataUpdate", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task PushMarketDataBatchAsync_PushesMultipleUpdates()
    {
        // Arrange
        var service = new MarketDataPushService(_mockHubContext.Object, _mockLogger.Object);
        var updates = new Dictionary<string, MarketDataUpdate>
        {
            { "AAPL", new MarketDataUpdate { Symbol = "AAPL", LastPrice = 150.50m, Timestamp = DateTime.UtcNow } },
            { "MSFT", new MarketDataUpdate { Symbol = "MSFT", LastPrice = 300.75m, Timestamp = DateTime.UtcNow } },
            { "GOOGL", new MarketDataUpdate { Symbol = "GOOGL", LastPrice = 2800.00m, Timestamp = DateTime.UtcNow } }
        };

        // Act
        await service.PushMarketDataBatchAsync(updates);

        // Assert - Should call Group for each symbol
        _mockClients.Verify(c => c.Group("MarketData:AAPL"), Times.Once);
        _mockClients.Verify(c => c.Group("MarketData:MSFT"), Times.Once);
        _mockClients.Verify(c => c.Group("MarketData:GOOGL"), Times.Once);
    }

    [Fact]
    public async Task PushMarketDataBatchAsync_WithEmptyDictionary_DoesNotSend()
    {
        // Arrange
        var service = new MarketDataPushService(_mockHubContext.Object, _mockLogger.Object);
        var updates = new Dictionary<string, MarketDataUpdate>();

        // Act
        await service.PushMarketDataBatchAsync(updates);

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
            Times.Never
        );
    }
}
