using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebUI.API.Hubs;

namespace WebUI.Tests.Hubs;

/// <summary>
/// Unit tests for OrderHub.
/// OrderHub 单元测试。
/// </summary>
public class OrderHubTests
{
    private readonly Mock<ILogger<OrderHub>> _mockLogger;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockCaller;

    public OrderHubTests()
    {
        _mockLogger = new Mock<ILogger<OrderHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockCaller = new Mock<ISingleClientProxy>();

        _mockClients.Setup(c => c.Caller).Returns(_mockCaller.Object);
    }

    private OrderHub CreateHub(string? userId = "test-user-id")
    {
        var hub = new OrderHub(_mockLogger.Object)
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
    public async Task SubscribeToOrderUpdates_WithValidUser_AddsToGroup()
    {
        // Arrange
        var hub = CreateHub();

        // Act
        await hub.SubscribeToOrderUpdates();

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "OrderUpdates:test-user-id", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("OrderSubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task SubscribeToOrderUpdates_WithoutUserId_SendsError()
    {
        // Arrange
        var hub = CreateHub(userId: null);

        // Act
        await hub.SubscribeToOrderUpdates();

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
    public async Task SubscribeToPositionUpdates_WithValidUser_AddsToGroup()
    {
        // Arrange
        var hub = CreateHub();

        // Act
        await hub.SubscribeToPositionUpdates();

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "PositionUpdates:test-user-id", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("PositionSubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task UnsubscribeFromOrderUpdates_RemovesFromGroup()
    {
        // Arrange
        var hub = CreateHub();
        await hub.SubscribeToOrderUpdates();

        // Act
        await hub.UnsubscribeFromOrderUpdates();

        // Assert
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync("test-connection-id", "OrderUpdates:test-user-id", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("OrderUnsubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task UnsubscribeFromPositionUpdates_RemovesFromGroup()
    {
        // Arrange
        var hub = CreateHub();
        await hub.SubscribeToPositionUpdates();

        // Act
        await hub.UnsubscribeFromPositionUpdates();

        // Assert
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync("test-connection-id", "PositionUpdates:test-user-id", default),
            Times.Once
        );
        _mockCaller.Verify(
            c => c.SendCoreAsync("PositionUnsubscriptionConfirmed", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task OnConnectedAsync_AutoSubscribesToOrdersAndPositions()
    {
        // Arrange
        var hub = CreateHub();

        // Act
        await hub.OnConnectedAsync();

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "OrderUpdates:test-user-id", default),
            Times.Once
        );
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", "PositionUpdates:test-user-id", default),
            Times.Once
        );
    }

    [Fact]
    public async Task OnDisconnectedAsync_CleansUpSubscriptions()
    {
        // Arrange
        var hub = CreateHub();
        await hub.OnConnectedAsync();

        // Act
        await hub.OnDisconnectedAsync(null);

        // Assert
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
