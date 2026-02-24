using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebUI.Data;
using WebUI.Data.Entities;
using WebUI.Data.Repositories;
using Xunit;

namespace WebUI.Tests.Repositories
{
    public class OrderRepositoryTests : IDisposable
    {
        private readonly WebUIDbContext _dbContext;
        private readonly OrderRepository _orderRepository;

        public OrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WebUIDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new WebUIDbContext(options);
            _orderRepository = new OrderRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task AddAsync_ValidOrder_ShouldAddToDatabase()
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

            // Act
            await _orderRepository.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Assert
            var savedOrder = await _dbContext.Orders.FindAsync(order.Id);
            savedOrder.Should().NotBeNull();
            savedOrder!.Symbol.Should().Be("AAPL");
        }

        [Fact]
        public async Task GetByIdAsync_ExistingOrder_ShouldReturnOrder()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Symbol = "MSFT",
                Quantity = 50,
                Side = "Sell",
                Type = "Limit",
                LimitPrice = 300.00m,
                Status = "Filled",
                UserId = Guid.NewGuid().ToString(),
                SubmittedAt = DateTime.UtcNow
            };
            await _orderRepository.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetByIdAsync(order.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Symbol.Should().Be("MSFT");
            result.LimitPrice.Should().Be(300.00m);
        }

        [Fact]
        public async Task GetAllAsync_MultipleOrders_ShouldReturnAll()
        {
            // Arrange
            var orders = new[]
            {
                new Order { Id = Guid.NewGuid(), Symbol = "AAPL", Quantity = 100, Side = "Buy", Type = "Market", Status = "Filled", UserId = Guid.NewGuid().ToString(), SubmittedAt = DateTime.UtcNow },
                new Order { Id = Guid.NewGuid(), Symbol = "GOOGL", Quantity = 50, Side = "Sell", Type = "Limit", Status = "Pending", UserId = Guid.NewGuid().ToString(), SubmittedAt = DateTime.UtcNow },
                new Order { Id = Guid.NewGuid(), Symbol = "TSLA", Quantity = 75, Side = "Buy", Type = "Market", Status = "Cancelled", UserId = Guid.NewGuid().ToString(), SubmittedAt = DateTime.UtcNow }
            };

            foreach (var order in orders)
            {
                await _orderRepository.AddAsync(order);
            }
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task UpdateAsync_ExistingOrder_ShouldModifyOrder()
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
            await _orderRepository.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            order.Status = "Filled";
            order.FilledAt = DateTime.UtcNow;
            _orderRepository.Update(order);
            await _dbContext.SaveChangesAsync();

            // Assert
            var updatedOrder = await _orderRepository.GetByIdAsync(order.Id);
            updatedOrder!.Status.Should().Be("Filled");
            updatedOrder.FilledAt.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteAsync_ExistingOrder_ShouldRemoveFromDatabase()
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
                SubmittedAt = DateTime.UtcNow
            };
            await _orderRepository.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            _orderRepository.Delete(order);
            await _dbContext.SaveChangesAsync();

            // Assert
            var deletedOrder = await _orderRepository.GetByIdAsync(order.Id);
            deletedOrder.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveOrdersByUserIdAsync_ShouldReturnOnlyActiveOrders()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var orders = new[]
            {
                new Order { Id = Guid.NewGuid(), Symbol = "AAPL", Quantity = 100, Side = "Buy", Type = "Limit", Status = "Pending", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Id = Guid.NewGuid(), Symbol = "MSFT", Quantity = 50, Side = "Buy", Type = "Limit", Status = "PartiallyFilled", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Id = Guid.NewGuid(), Symbol = "GOOGL", Quantity = 25, Side = "Sell", Type = "Market", Status = "Filled", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Id = Guid.NewGuid(), Symbol = "TSLA", Quantity = 75, Side = "Buy", Type = "Limit", Status = "Cancelled", UserId = userId, SubmittedAt = DateTime.UtcNow }
            };

            foreach (var order in orders)
            {
                await _orderRepository.AddAsync(order);
            }
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetActiveOrdersByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(o => o.Status == "Pending");
            result.Should().Contain(o => o.Status == "PartiallyFilled");
        }

        [Fact]
        public async Task GetOrdersBySymbolAsync_ShouldReturnOnlyMatchingSymbol()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var orders = new[]
            {
                new Order { Id = Guid.NewGuid(), Symbol = "AAPL", Quantity = 100, Side = "Buy", Type = "Market", Status = "Filled", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Id = Guid.NewGuid(), Symbol = "AAPL", Quantity = 50, Side = "Sell", Type = "Limit", Status = "Pending", UserId = userId, SubmittedAt = DateTime.UtcNow },
                new Order { Id = Guid.NewGuid(), Symbol = "MSFT", Quantity = 75, Side = "Buy", Type = "Market", Status = "Filled", UserId = userId, SubmittedAt = DateTime.UtcNow }
            };

            foreach (var order in orders)
            {
                await _orderRepository.AddAsync(order);
            }
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrdersBySymbolAsync("AAPL");

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(o => o.Symbol.Should().Be("AAPL"));
        }
    }

    public class PositionRepositoryTests : IDisposable
    {
        private readonly WebUIDbContext _dbContext;
        private readonly PositionRepository _positionRepository;

        public PositionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WebUIDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new WebUIDbContext(options);
            _positionRepository = new PositionRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetPositionsByUserIdAsync_ShouldReturnOnlyUserPositions()
        {
            // Arrange
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();

            var positions = new[]
            {
                new Position { Id = Guid.NewGuid(), Symbol = "AAPL", Quantity = 100, AverageCost = 150.00m, UserId = userId1, LastUpdated = DateTime.UtcNow },
                new Position { Id = Guid.NewGuid(), Symbol = "MSFT", Quantity = 50, AverageCost = 300.00m, UserId = userId1, LastUpdated = DateTime.UtcNow },
                new Position { Id = Guid.NewGuid(), Symbol = "GOOGL", Quantity = 25, AverageCost = 2800.00m, UserId = userId2, LastUpdated = DateTime.UtcNow }
            };

            foreach (var position in positions)
            {
                await _positionRepository.AddAsync(position);
            }
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _positionRepository.GetPositionsByUserIdAsync(userId1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.UserId.Should().Be(userId1));
        }

        [Fact]
        public async Task GetPositionBySymbolAsync_ShouldReturnMatchingPosition()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var position = new Position
            {
                Id = Guid.NewGuid(),
                Symbol = "AAPL",
                Quantity = 100,
                AverageCost = 150.00m,
                UserId = userId,
                LastUpdated = DateTime.UtcNow
            };
            await _positionRepository.AddAsync(position);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _positionRepository.GetPositionBySymbolAsync(userId, "AAPL");

            // Assert
            result.Should().NotBeNull();
            result!.Symbol.Should().Be("AAPL");
            result.Quantity.Should().Be(100);
        }
    }

    public class StrategyRepositoryTests : IDisposable
    {
        private readonly WebUIDbContext _dbContext;
        private readonly StrategyRepository _strategyRepository;

        public StrategyRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WebUIDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new WebUIDbContext(options);
            _strategyRepository = new StrategyRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetActiveStrategiesAsync_ShouldReturnOnlyRunningStrategies()
        {
            // Arrange
            var strategies = new[]
            {
                new Strategy { Id = Guid.NewGuid(), Name = "Strategy1", Status = "Running", UserId = Guid.NewGuid().ToString(), CreatedAt = DateTime.UtcNow },
                new Strategy { Id = Guid.NewGuid(), Name = "Strategy2", Status = "Stopped", UserId = Guid.NewGuid().ToString(), CreatedAt = DateTime.UtcNow },
                new Strategy { Id = Guid.NewGuid(), Name = "Strategy3", Status = "Running", UserId = Guid.NewGuid().ToString(), CreatedAt = DateTime.UtcNow }
            };

            foreach (var strategy in strategies)
            {
                await _strategyRepository.AddAsync(strategy);
            }
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _strategyRepository.GetActiveStrategiesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(s => s.Status.Should().Be("Running"));
        }

        [Fact]
        public async Task GetStrategiesByUserIdAsync_ShouldReturnOnlyUserStrategies()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var otherUserId = Guid.NewGuid().ToString();

            var strategies = new[]
            {
                new Strategy { Id = Guid.NewGuid(), Name = "MyStrategy1", Status = "Running", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Strategy { Id = Guid.NewGuid(), Name = "MyStrategy2", Status = "Stopped", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Strategy { Id = Guid.NewGuid(), Name = "OtherStrategy", Status = "Running", UserId = otherUserId, CreatedAt = DateTime.UtcNow }
            };

            foreach (var strategy in strategies)
            {
                await _strategyRepository.AddAsync(strategy);
            }
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _strategyRepository.GetStrategiesByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(s => s.UserId.Should().Be(userId));
        }
    }
}
