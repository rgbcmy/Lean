using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Data;
using WebUI.Data.Entities;
using WebUI.Data.Services;
using Xunit;

namespace WebUI.Tests.Services
{
    public class AuditLogServiceTests : IDisposable
    {
        private readonly WebUIDbContext _dbContext;
        private readonly Mock<ILogger<AuditLogService>> _loggerMock;
        private readonly IAuditLogService _auditLogService;

        public AuditLogServiceTests()
        {
            var options = new DbContextOptionsBuilder<WebUIDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new WebUIDbContext(options);
            _loggerMock = new Mock<ILogger<AuditLogService>>();
            _auditLogService = new AuditLogService(_dbContext, _loggerMock.Object);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task LogAsync_ValidEntry_ShouldCreateAuditLog()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var action = "LOGIN";
            var details = "User logged in successfully";

            // Act
            await _auditLogService.LogAsync(userId, action, details);

            // Assert
            var logs = await _dbContext.AuditLogs.ToListAsync();
            logs.Should().HaveCount(1);
            logs[0].UserId.Should().Be(userId);
            logs[0].Action.Should().Be(action);
            logs[0].Details.Should().Be(details);
            logs[0].Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task LogAsync_WithIpAddress_ShouldStoreIpAddress()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var action = "ORDER_CREATE";
            var details = "Created order for AAPL";
            var ipAddress = "192.168.1.100";

            // Act
            await _auditLogService.LogAsync(userId, action, details, ipAddress);

            // Assert
            var logs = await _dbContext.AuditLogs.ToListAsync();
            logs.Should().HaveCount(1);
            logs[0].IpAddress.Should().Be(ipAddress);
        }

        [Fact]
        public async Task GetLogsByUserIdAsync_ShouldReturnOnlyUserLogs()
        {
            // Arrange
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();

            await _auditLogService.LogAsync(userId1, "LOGIN", "User 1 login");
            await _auditLogService.LogAsync(userId1, "ORDER_CREATE", "User 1 order");
            await _auditLogService.LogAsync(userId2, "LOGIN", "User 2 login");

            // Act
            var result = await _auditLogService.GetLogsByUserIdAsync(userId1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(log => log.UserId.Should().Be(userId1));
        }

        [Fact]
        public async Task GetLogsByActionAsync_ShouldReturnOnlyMatchingAction()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            await _auditLogService.LogAsync(userId, "LOGIN", "Login 1");
            await _auditLogService.LogAsync(userId, "ORDER_CREATE", "Order 1");
            await _auditLogService.LogAsync(userId, "LOGIN", "Login 2");
            await _auditLogService.LogAsync(userId, "ORDER_CANCEL", "Cancel 1");

            // Act
            var result = await _auditLogService.GetLogsByActionAsync("LOGIN");

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(log => log.Action.Should().Be("LOGIN"));
        }

        [Fact]
        public async Task GetLogsInDateRangeAsync_ShouldReturnLogsWithinRange()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;
            var startDate = now.AddHours(-2);
            var endDate = now.AddHours(2);

            // Create logs with different timestamps
            var oldLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "OLD_ACTION",
                Details = "Old log",
                Timestamp = now.AddHours(-3)  // Outside range
            };

            var recentLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "RECENT_ACTION",
                Details = "Recent log",
                Timestamp = now  // Inside range
            };

            _dbContext.AuditLogs.AddRange(oldLog, recentLog);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _auditLogService.GetLogsInDateRangeAsync(startDate, endDate);

            // Assert
            result.Should().HaveCount(1);
            result[0].Action.Should().Be("RECENT_ACTION");
        }

        [Theory]
        [InlineData("ORDER_CREATE", "Created order")]
        [InlineData("ORDER_CANCEL", "Cancelled order")]
        [InlineData("STRATEGY_START", "Started strategy")]
        [InlineData("STRATEGY_STOP", "Stopped strategy")]
        public async Task LogAsync_DifferentActions_ShouldCreateAllLogs(string action, string details)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            // Act
            await _auditLogService.LogAsync(userId, action, details);

            // Assert
            var logs = await _dbContext.AuditLogs.Where(l => l.Action == action).ToListAsync();
            logs.Should().HaveCount(1);
            logs[0].Details.Should().Be(details);
        }

        [Fact]
        public async Task GetRecentLogsAsync_ShouldReturnMostRecentLogs()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;

            for (int i = 0; i < 10; i++)
            {
                await _auditLogService.LogAsync(userId, $"ACTION_{i}", $"Details {i}");
                await Task.Delay(10); // Ensure different timestamps
            }

            // Act
            var result = await _auditLogService.GetRecentLogsAsync(5);

            // Assert
            result.Should().HaveCount(5);
            result.Should().BeInDescendingOrder(log => log.Timestamp);
        }

        [Fact]
        public async Task DeleteOldLogsAsync_ShouldRemoveLogsOlderThanDate()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;
            var cutoffDate = now.AddDays(-30);

            var oldLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "OLD",
                Details = "Old log",
                Timestamp = now.AddDays(-60)
            };

            var recentLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "RECENT",
                Details = "Recent log",
                Timestamp = now
            };

            _dbContext.AuditLogs.AddRange(oldLog, recentLog);
            await _dbContext.SaveChangesAsync();

            // Act
            var deletedCount = await _auditLogService.DeleteOldLogsAsync(cutoffDate);

            // Assert
            deletedCount.Should().Be(1);
            var remainingLogs = await _dbContext.AuditLogs.ToListAsync();
            remainingLogs.Should().HaveCount(1);
            remainingLogs[0].Action.Should().Be("RECENT");
        }
    }
}
