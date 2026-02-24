using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Core.Models.Backtest;
using WebUI.Data;
using WebUI.Data.Entities;
using WebUI.Data.Services;
using Xunit;

namespace WebUI.Tests.Services
{
    /// <summary>
    /// Unit tests for BacktestService / BacktestService 单元测试
    /// </summary>
    public class BacktestServiceTests : IDisposable
    {
        private readonly WebUIDbContext _context;
        private readonly Mock<ILogger<BacktestService>> _loggerMock;
        private readonly BacktestService _service;
        private readonly int _testUserId = 1;
        private readonly int _testStrategyId = 1;

        public BacktestServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<WebUIDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WebUIDbContext(options);
            _loggerMock = new Mock<ILogger<BacktestService>>();
            _service = new BacktestService(_context, _loggerMock.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var user = new User
            {
                Id = _testUserId,
                Username = "testuser",
                PasswordHash = "hash",
                Email = "test@example.com"
            };

            var strategy = new Strategy
            {
                Id = _testStrategyId,
                UserId = _testUserId,
                Name = "Test Strategy",
                StrategyType = "TestStrategy",
                ConfigurationJson = "{\"param1\":\"value1\"}",
                User = user
            };

            _context.Users.Add(user);
            _context.Strategies.Add(strategy);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateBacktestAsync_ValidRequest_CreatesBacktest()
        {
            // Arrange
            var request = new CreateBacktestRequest
            {
                StrategyId = _testStrategyId,
                Name = "Test Backtest",
                Description = "Test Description",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                BenchmarkSymbol = "SPY",
                DataResolution = "Daily"
            };

            // Act
            var result = await _service.CreateBacktestAsync(request, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.StrategyId, result.StrategyId);
            Assert.Equal("Pending", result.Status);
            Assert.Equal(request.InitialCapital, result.InitialCapital);

            // Verify database
            var backtest = await _context.Backtests.FirstOrDefaultAsync(b => b.Id == result.Id);
            Assert.NotNull(backtest);
            Assert.Equal(request.Name, backtest.Name);
        }

        [Fact]
        public async Task CreateBacktestAsync_InvalidDateRange_ThrowsException()
        {
            // Arrange
            var request = new CreateBacktestRequest
            {
                StrategyId = _testStrategyId,
                Name = "Test Backtest",
                StartDate = new DateTime(2021, 1, 1),
                EndDate = new DateTime(2020, 1, 1), // End before start
                InitialCapital = 100000,
                DataResolution = "Daily"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateBacktestAsync(request, _testUserId));
        }

        [Fact]
        public async Task CreateBacktestAsync_NonExistentStrategy_ThrowsException()
        {
            // Arrange
            var request = new CreateBacktestRequest
            {
                StrategyId = 999, // Non-existent
                Name = "Test Backtest",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                DataResolution = "Daily"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateBacktestAsync(request, _testUserId));
        }

        [Fact]
        public async Task GetBacktestsAsync_ReturnsAllBacktests()
        {
            // Arrange
            var backtest1 = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Backtest 1",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                DataResolution = "Daily",
                Status = "Completed"
            };

            var backtest2 = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Backtest 2",
                StartDate = new DateTime(2021, 1, 1),
                EndDate = new DateTime(2022, 1, 1),
                InitialCapital = 200000,
                DataResolution = "Daily",
                Status = "Pending"
            };

            _context.Backtests.AddRange(backtest1, backtest2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBacktestsAsync(_testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
            Assert.Equal(2, result.Backtests.Length);
            Assert.Contains(result.Backtests, b => b.Name == "Backtest 1");
            Assert.Contains(result.Backtests, b => b.Name == "Backtest 2");
        }

        [Fact]
        public async Task GetBacktestsAsync_FilterByStatus_ReturnsFilteredBacktests()
        {
            // Arrange
            var backtest1 = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Completed Backtest",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                DataResolution = "Daily",
                Status = "Completed"
            };

            var backtest2 = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Pending Backtest",
                StartDate = new DateTime(2021, 1, 1),
                EndDate = new DateTime(2022, 1, 1),
                InitialCapital = 200000,
                DataResolution = "Daily",
                Status = "Pending"
            };

            _context.Backtests.AddRange(backtest1, backtest2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBacktestsAsync(_testUserId, status: "Completed");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.Equal("Completed Backtest", result.Backtests[0].Name);
        }

        [Fact]
        public async Task GetBacktestByIdAsync_ExistingBacktest_ReturnsBacktest()
        {
            // Arrange
            var backtest = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Test Backtest",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                DataResolution = "Daily",
                Status = "Completed",
                TotalReturn = 0.15m,
                SharpeRatio = 1.5m
            };

            _context.Backtests.Add(backtest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetBacktestByIdAsync(backtest.Id, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(backtest.Name, result.Name);
            Assert.Equal(backtest.TotalReturn, result.Performance?.TotalReturn);
            Assert.Equal(backtest.SharpeRatio, result.Performance?.SharpeRatio);
        }

        [Fact]
        public async Task GetBacktestByIdAsync_NonExistentBacktest_ReturnsNull()
        {
            // Act
            var result = await _service.GetBacktestByIdAsync(999, _testUserId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteBacktestAsync_ExistingBacktest_DeletesBacktest()
        {
            // Arrange
            var backtest = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Test Backtest",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                DataResolution = "Daily",
                Status = "Completed"
            };

            _context.Backtests.Add(backtest);
            await _context.SaveChangesAsync();
            var backtestId = backtest.Id;

            // Act
            var result = await _service.DeleteBacktestAsync(backtestId, _testUserId);

            // Assert
            Assert.True(result);
            var deletedBacktest = await _context.Backtests.FindAsync(backtestId);
            Assert.Null(deletedBacktest);
        }

        [Fact]
        public async Task UpdateBacktestStatusAsync_ValidBacktest_UpdatesStatus()
        {
            // Arrange
            var backtest = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Test Backtest",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                DataResolution = "Daily",
                Status = "Pending"
            };

            _context.Backtests.Add(backtest);
            await _context.SaveChangesAsync();

            // Act
            await _service.UpdateBacktestStatusAsync(backtest.Id, "Running", 50);

            // Assert
            var updated = await _context.Backtests.FindAsync(backtest.Id);
            Assert.NotNull(updated);
            Assert.Equal("Running", updated.Status);
            Assert.Equal(50, updated.Progress);
            Assert.NotNull(updated.StartedAt);
        }

        [Fact]
        public async Task CompareBacktestsAsync_ValidBacktests_ReturnsComparison()
        {
            // Arrange
            var backtest1 = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Backtest 1",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                DataResolution = "Daily",
                Status = "Completed",
                SharpeRatio = 1.2m,
                TotalReturn = 0.15m
            };

            var backtest2 = new Backtest
            {
                UserId = _testUserId,
                StrategyId = _testStrategyId,
                Name = "Backtest 2",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1),
                InitialCapital = 100000,
                DataResolution = "Daily",
                Status = "Completed",
                SharpeRatio = 1.5m, // Better
                TotalReturn = 0.20m
            };

            _context.Backtests.AddRange(backtest1, backtest2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CompareBacktestsAsync(
                new[] { backtest1.Id, backtest2.Id }, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Backtests.Length);
            Assert.Equal(backtest2.Id, result.BestBacktestId);
            Assert.Equal("SharpeRatio", result.BestBy);
            Assert.True(result.Backtests.First(b => b.Id == backtest2.Id).IsBest);
        }

        [Fact]
        public async Task CompareBacktestsAsync_TooFewBacktests_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CompareBacktestsAsync(new[] { 1 }, _testUserId));
        }

        [Fact]
        public async Task CompareBacktestsAsync_TooManyBacktests_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CompareBacktestsAsync(new[] { 1, 2, 3, 4, 5 }, _testUserId));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
