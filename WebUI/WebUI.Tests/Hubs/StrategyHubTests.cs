using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebUI.API.Hubs;

namespace WebUI.Tests.Hubs;

/// <summary>
/// Unit tests for StrategyHub.
/// StrategyHub 单元测试。
/// </summary>
public class StrategyHubTests
{
    private readonly Mock<ILogger<StrategyHub>> _mockLogger;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockCaller;

    public StrategyHubTests()
    {
        _mockLogger = new Mock<ILogger<StrategyHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockCaller = new Mock<ISingleClientProxy>();

        _mockClients.Setup(c => c.Caller).Returns(_mockCaller.Object);
    }

    private StrategyHub CreateHub(string? userId = "test-user-id")
    {
        var hub = new StrategyHub(_mockLogger.Object)
        {
            Context = _mockContext.Object,
            Groups = _mockGroups.Object,
            Clients = _mockClients.Object
        };

        // Setup user claims
        if (userId != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _mockContext.Setup(c => c.User).Returns(principal);
        }
        else
        {
            _mockContext.Setup(c => c.User).Returns((ClaimsPrincipal)null!);
        }

        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");

        return hub;
    }

    [Fact]
    public async Task SubscribeToStrategyLogs_WithValidStrategies_AddsToGroups()
    {
        // Arrange
        var hub = CreateHub();
        var strategyIds = new[] { "strategy-1", "strategy-2" };

        // Act
        await hub.SubscribeToStrategyLogs(strategyIds);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "StrategyLog:strategy-1", default),
            Times.Once
        );
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "StrategyLog:strategy-2", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("StrategySubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task SubscribeToStrategyLogs_WithEmptyArray_DoesNotAddToGroups()
    {
        // Arrange
        var hub = CreateHub();
        var strategyIds = Array.Empty<string>();

        // Act
        await hub.SubscribeToStrategyLogs(strategyIds);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Never
        );
    }

    [Fact]
    public async Task SubscribeToStrategyLogs_WithoutUserId_SendsError()
    {
        // Arrange
        var hub = CreateHub(userId: null);
        var strategyIds = new[] { "strategy-1" };

        // Act
        await hub.SubscribeToStrategyLogs(strategyIds);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Never
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("Error", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task UnsubscribeFromStrategyLogs_RemovesFromGroups()
    {
        // Arrange
        var hub = CreateHub();
        var strategyIds = new[] { "strategy-1", "strategy-2" };
        await hub.SubscribeToStrategyLogs(strategyIds);

        // Act
        await hub.UnsubscribeFromStrategyLogs(strategyIds);

        // Assert
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync("test-connection-id", "StrategyLog:strategy-1", default),
            Times.Once
        );
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync("test-connection-id", "StrategyLog:strategy-2", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("StrategyUnsubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task SubscribeToStrategyStatus_WithValidStrategies_AddsToGroups()
    {
        // Arrange
        var hub = CreateHub();
        var strategyIds = new[] { "strategy-1", "strategy-2" };

        // Act
        await hub.SubscribeToStrategyStatus(strategyIds);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "StrategyStatus:strategy-1", default),
            Times.Once
        );
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "StrategyStatus:strategy-2", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("StatusSubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task GetSubscriptions_ReturnsCurrentSubscriptions()
    {
        // Arrange
        var hub = CreateHub();
        var strategyIds = new[] { "strategy-1", "strategy-2" };
        await hub.SubscribeToStrategyLogs(strategyIds);

        // Act
        await hub.GetSubscriptions();

        // Assert
        _mockCaller.Verify(
            c => c.SendCoreAsync("CurrentStrategySubscriptions", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task OnDisconnectedAsync_CleansUpSubscriptions()
    {
        // Arrange
        var hub = CreateHub();
        var strategyIds = new[] { "strategy-1", "strategy-2" };
        await hub.SubscribeToStrategyLogs(strategyIds);

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
