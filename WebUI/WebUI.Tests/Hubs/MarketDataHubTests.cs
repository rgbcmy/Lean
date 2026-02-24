using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebUI.API.Hubs;

namespace WebUI.Tests.Hubs;

/// <summary>
/// Unit tests for MarketDataHub.
/// MarketDataHub 单元测试。
/// </summary>
public class MarketDataHubTests
{
    private readonly Mock<ILogger<MarketDataHub>> _mockLogger;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockCaller;

    public MarketDataHubTests()
    {
        _mockLogger = new Mock<ILogger<MarketDataHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockCaller = new Mock<ISingleClientProxy>();

        _mockClients.Setup(c => c.Caller).Returns(_mockCaller.Object);
    }

    private MarketDataHub CreateHub()
    {
        var hub = new MarketDataHub(_mockLogger.Object)
        {
            Context = _mockContext.Object,
            Groups = _mockGroups.Object,
            Clients = _mockClients.Object
        };

        // Setup default user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");

        return hub;
    }

    [Fact]
    public async Task SubscribeToMarketData_WithValidSymbols_AddsToGroups()
    {
        // Arrange
        var hub = CreateHub();
        var symbols = new[] { "AAPL", "MSFT" };

        // Act
        await hub.SubscribeToMarketData(symbols);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "MarketData:AAPL", default),
            Times.Once
        );
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "MarketData:MSFT", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("SubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task SubscribeToMarketData_WithEmptyArray_DoesNotAddToGroups()
    {
        // Arrange
        var hub = CreateHub();
        var symbols = Array.Empty<string>();

        // Act
        await hub.SubscribeToMarketData(symbols);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Never
        );
    }

    [Fact]
    public async Task SubscribeToMarketData_WithLowercaseSymbols_NormalizesToUppercase()
    {
        // Arrange
        var hub = CreateHub();
        var symbols = new[] { "aapl", "msft" };

        // Act
        await hub.SubscribeToMarketData(symbols);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "MarketData:AAPL", default),
            Times.Once
        );
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "MarketData:MSFT", default),
            Times.Once
        );
    }

    [Fact]
    public async Task UnsubscribeFromMarketData_WithValidSymbols_RemovesFromGroups()
    {
        // Arrange
        var hub = CreateHub();
        var symbols = new[] { "AAPL", "MSFT" };

        // Subscribe first
        await hub.SubscribeToMarketData(symbols);

        // Act
        await hub.UnsubscribeFromMarketData(symbols);

        // Assert
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync("test-connection-id", "MarketData:AAPL", default),
            Times.Once
        );
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync("test-connection-id", "MarketData:MSFT", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("UnsubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task GetSubscriptions_ReturnsCurrentSubscriptions()
    {
        // Arrange
        var hub = CreateHub();
        var symbols = new[] { "AAPL", "MSFT", "GOOGL" };
        await hub.SubscribeToMarketData(symbols);

        // Act
        await hub.GetSubscriptions();

        // Assert
        _mockCaller.Verify(
            c => c.SendCoreAsync("CurrentSubscriptions", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task OnConnectedAsync_LogsConnection()
    {
        // Arrange
        var hub = CreateHub();

        // Act
        await hub.OnConnectedAsync();

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Client connected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task OnDisconnectedAsync_CleansUpSubscriptions()
    {
        // Arrange
        var hub = CreateHub();
        var symbols = new[] { "AAPL", "MSFT" };
        await hub.SubscribeToMarketData(symbols);

        // Act
        await hub.OnDisconnectedAsync(null);

        // Assert
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync("test-connection-id", It.IsAny<string>(), default),
            Times.AtLeastOnce
        );
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleaned up")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
