using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Data.Services;
using WebUI.Data.Entities;
using WebUI.Data.Models.Trading;
using WebUI.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace WebUI.Tests.Services
{
    public class TradingServiceTests : IDisposable
    {
        private readonly WebUIDbContext _dbContext;
        private readonly Mock<ILogger<TradingService>> _loggerMock;
        private readonly ITradingService _tradingService;

        public TradingServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<WebUIDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new WebUIDbContext(options);
            _loggerMock = new Mock<ILogger<TradingService>>();

            _tradingService = new TradingService(
                _dbContext,
                _loggerMock.Object
            );
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task CreateOrderAsync_ValidOrder_ShouldCreateSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                Symbol = "AAPL",
                Quantity = 100,
                Side = "Buy",
                Type = "Market",
                Status = "Pending",
                UserId = Guid.NewGuid().ToString(),
                SubmittedAt = DateTime.UtcNow
            };

            // Act
            var result = await _tradingService.CreateOrderAsync(order);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Symbol.Should().Be("AAPL");
            result.Quantity.Should().Be(100);

            var savedOrder = await _dbContext.Orders.FindAsync(result.Id);
            savedOrder.Should().NotBeNull();
            savedOrder!.Symbol.Should().Be("AAPL");
        }

        [Fact]
        public async Task GetOrderByIdAsync_ExistingOrder_ShouldReturnOrder()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Symbol = "TSLA",
                Quantity = 50,
                Side = "Sell",
                Type = "Limit",
                LimitPrice = 250.00m,
                Status = "Filled",
                UserId = Guid.NewGuid().ToString(),
                SubmittedAt = DateTime.UtcNow
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tradingService.GetOrderByIdAsync(order.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Symbol.Should().Be("TSLA");
            result.Quantity.Should().Be(50);
            result.LimitPrice.Should().Be(250.00m);
        }

        [Fact]
        public async Task GetOrderByIdAsync_NonExistingOrder_ShouldReturnNull()
        {
            // Act
            var result = await _tradingService.GetOrderByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetOrdersByUserIdAsync_ShouldReturnOnlyUserOrders()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var otherUserId = Guid.NewGuid().ToString();

            var userOrders = new[]
            {
                new Order { Symbol = "AAPL", Quantity = 100, Side = "Buy", Type = "Market", Status = "Filled", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Symbol = "MSFT", Quantity = 50, Side = "Buy", Type = "Limit", Status = "Pending", UserId = userId, SubmittedAt = DateTime.UtcNow }
            };

            var otherOrder = new Order { Symbol = "GOOGL", Quantity = 25, Side = "Sell", Type = "Market", Status = "Filled", UserId = otherUserId, SubmittedAt = DateTime.UtcNow };

            _dbContext.Orders.AddRange(userOrders);
            _dbContext.Orders.Add(otherOrder);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tradingService.GetOrdersByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(o => o.UserId.Should().Be(userId));
            result.Should().Contain(o => o.Symbol == "AAPL");
            result.Should().Contain(o => o.Symbol == "MSFT");
        }

        [Fact]
        public async Task CancelOrderAsync_PendingOrder_ShouldUpdateStatusToCancelled()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Symbol = "AAPL",
                Quantity = 100,
                Side = "Buy",
                Type = "Limit",
                LimitPrice = 150.00m,
                Status = "Pending",
                UserId = Guid.NewGuid().ToString(),
                SubmittedAt = DateTime.UtcNow
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tradingService.CancelOrderAsync(order.Id);

            // Assert
            result.Should().BeTrue();
            var cancelledOrder = await _dbContext.Orders.FindAsync(order.Id);
            cancelledOrder!.Status.Should().Be("Cancelled");
            cancelledOrder.CancelledAt.Should().NotBeNull();
        }

        [Fact]
        public async Task CancelOrderAsync_FilledOrder_ShouldReturnFalse()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Symbol = "AAPL",
                Quantity = 100,
                Side = "Buy",
                Type = "Market",
                Status = "Filled",
                UserId = Guid.NewGuid().ToString(),
                SubmittedAt = DateTime.UtcNow,
                FilledAt = DateTime.UtcNow
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tradingService.CancelOrderAsync(order.Id);

            // Assert
            result.Should().BeFalse();
            var unchangedOrder = await _dbContext.Orders.FindAsync(order.Id);
            unchangedOrder!.Status.Should().Be("Filled");
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldUpdateStatus()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Symbol = "AAPL",
                Quantity = 100,
                Side = "Buy",
                Type = "Market",
                Status = "Pending",
                UserId = Guid.NewGuid().ToString(),
                SubmittedAt = DateTime.UtcNow
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            await _tradingService.UpdateOrderStatusAsync(order.Id, "PartiallyFilled");

            // Assert
            var updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
            updatedOrder!.Status.Should().Be("PartiallyFilled");
        }

        [Fact]
        public async Task GetActiveOrdersAsync_ShouldReturnOnlyActiveOrders()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var orders = new[]
            {
                new Order { Symbol = "AAPL", Quantity = 100, Side = "Buy", Type = "Limit", Status = "Pending", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Symbol = "MSFT", Quantity = 50, Side = "Buy", Type = "Limit", Status = "PartiallyFilled", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Symbol = "GOOGL", Quantity = 25, Side = "Sell", Type = "Market", Status = "Filled", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Symbol = "TSLA", Quantity = 75, Side = "Buy", Type = "Limit", Status = "Cancelled", UserId = userId, SubmittedAt = DateTime.UtcNow }
            };
            _dbContext.Orders.AddRange(orders);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tradingService.GetActiveOrdersAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(o => o.Status == "Pending");
            result.Should().Contain(o => o.Status == "PartiallyFilled");
            result.Should().NotContain(o => o.Status == "Filled");
            result.Should().NotContain(o => o.Status == "Cancelled");
        }
    }
}
