using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using WebUI.Core.Models;
using WebUI.Data;
using WebUI.Data.Entities;
using WebUI.Data.Services;
using Xunit;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace WebUI.Tests.Services;

/// <summary>
/// Unit tests for StrategyService / 策略服务单元测试
/// </summary>
public class StrategyServiceTests : IDisposable
{
    private readonly Mock<ILogger<StrategyService>> _mockLogger;
    private readonly WebUIDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly StrategyService _strategyService;
    private readonly string _testStrategiesDir;

    public StrategyServiceTests()
    {
        _mockLogger = new Mock<ILogger<StrategyService>>();

        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<WebUIDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new WebUIDbContext(options);
        _unitOfWork = new UnitOfWork(_dbContext);

        // Create temporary test strategies directory
        _testStrategiesDir = Path.Combine(Path.GetTempPath(), "StrategyTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testStrategiesDir);

        // Seed test data
        SeedTestData();

        _strategyService = new StrategyService(
            _mockLogger.Object,
            _unitOfWork);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();

        // Clean up test strategies directory
        if (Directory.Exists(_testStrategiesDir))
        {
            Directory.Delete(_testStrategiesDir, true);
        }
    }

    private void SeedTestData()
    {
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };

        var strategy1 = new Strategy
        {
            Id = 1,
            UserId = 1,
            Name = "Test Strategy 1",
            Description = "Test description",
            StrategyType = "Custom",
            Tags = "test,momentum",
            IsActive = false,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        var strategy2 = new Strategy
        {
            Id = 2,
            UserId = 1,
            Name = "Test Strategy 2",
            Description = "Another test",
            StrategyType = "Template",
            Tags = "test,mean-reversion",
            IsActive = true,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        var execution1 = new StrategyExecution
        {
            Id = 1,
            StrategyId = 1,
            StartedAt = DateTime.UtcNow.AddDays(-1),
            StoppedAt = DateTime.UtcNow,
            Status = "Completed",
            ProcessId = 12345,
            TotalReturn = 5.5m,
            OrdersExecuted = 10
        };

        _dbContext.Users.Add(user);
        _dbContext.Strategies.Add(strategy1);
        _dbContext.Strategies.Add(strategy2);
        _dbContext.StrategyExecutions.Add(execution1);
        _dbContext.SaveChanges();
    }

    #region GetStrategiesAsync Tests

    [Fact]
    public async Task GetStrategiesAsync_WithNoFilter_ReturnsAllStrategies()
    {
        // Act
        var result = await _strategyService.GetStrategiesAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Strategies.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStrategiesAsync_WithActiveFilter_ReturnsOnlyActiveStrategies()
    {
        // Act
        var result = await _strategyService.GetStrategiesAsync(1, "active");

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Strategies.Should().HaveCount(1);
        result.Strategies.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetStrategiesAsync_WithInactiveFilter_ReturnsOnlyInactiveStrategies()
    {
        // Act
        var result = await _strategyService.GetStrategiesAsync(1, "inactive");

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Strategies.Should().HaveCount(1);
        result.Strategies.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetStrategiesAsync_IncludesLastRunTimeAndCumulativeReturn()
    {
        // Act
        var result = await _strategyService.GetStrategiesAsync(1);

        // Assert
        var strategyWithExecution = result.Strategies.FirstOrDefault(s => s.Id == 1);
        strategyWithExecution.Should().NotBeNull();
        strategyWithExecution!.LastRunTime.Should().NotBeNull();
        strategyWithExecution.CumulativeReturn.Should().Be(5.5m);
    }

    #endregion

    #region GetStrategyByIdAsync Tests

    [Fact]
    public async Task GetStrategyByIdAsync_WithValidId_ReturnsStrategy()
    {
        // Act
        var result = await _strategyService.GetStrategyByIdAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Strategy 1");
    }

    [Fact]
    public async Task GetStrategyByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _strategyService.GetStrategyByIdAsync(999, 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStrategyByIdAsync_WithWrongUserId_ReturnsNull()
    {
        // Act
        var result = await _strategyService.GetStrategyByIdAsync(1, 999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStrategyByIdAsync_IncludesRecentExecutions()
    {
        // Act
        var result = await _strategyService.GetStrategyByIdAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result!.RecentExecutions.Should().HaveCount(1);
        result.RecentExecutions.First().Status.Should().Be("Completed");
    }

    #endregion

    #region CreateStrategyAsync Tests

    [Fact]
    public async Task CreateStrategyAsync_WithValidRequest_CreatesStrategy()
    {
        // Arrange
        var request = new CreateStrategyRequest
        {
            Name = "New Strategy",
            Description = "New description",
            StrategyType = "Custom",
            Tags = "new,test"
        };

        // Act
        var result = await _strategyService.CreateStrategyAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Strategy");
        result.Version.Should().Be(1);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateStrategyAsync_WithDuplicateName_ThrowsException()
    {
        // Arrange
        var request = new CreateStrategyRequest
        {
            Name = "Test Strategy 1", // Already exists
            StrategyType = "Custom"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.CreateStrategyAsync(1, request);
        });
    }

    [Fact]
    public async Task CreateStrategyAsync_CreatesInitialVersion()
    {
        // Arrange
        var request = new CreateStrategyRequest
        {
            Name = "Strategy With Version",
            StrategyType = "Custom"
        };

        // Act
        var result = await _strategyService.CreateStrategyAsync(1, request);

        // Assert
        var versions = await _strategyService.GetStrategyVersionsAsync(result.Id, 1);
        versions.Should().HaveCount(1);
        versions.First().VersionNumber.Should().Be(1);
    }

    #endregion

    #region UpdateStrategyAsync Tests

    [Fact]
    public async Task UpdateStrategyAsync_WithValidRequest_UpdatesStrategy()
    {
        // Arrange
        var request = new UpdateStrategyRequest
        {
            Name = "Updated Name",
            Description = "Updated description"
        };

        // Act
        var result = await _strategyService.UpdateStrategyAsync(1, 1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated description");
        result.Version.Should().Be(2); // Version should increment
    }

    [Fact]
    public async Task UpdateStrategyAsync_WithActiveStrategy_ThrowsException()
    {
        // Arrange
        var request = new UpdateStrategyRequest { Name = "New Name" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.UpdateStrategyAsync(2, 1, request); // Strategy 2 is active
        });
    }

    [Fact]
    public async Task UpdateStrategyAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var request = new UpdateStrategyRequest { Name = "New Name" };

        // Act
        var result = await _strategyService.UpdateStrategyAsync(999, 1, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStrategyAsync_CreatesNewVersion()
    {
        // Arrange
        var request = new UpdateStrategyRequest
        {
            ConfigurationJson = "{\"param1\": \"value1\"}"
        };

        // Act
        await _strategyService.UpdateStrategyAsync(1, 1, request);

        // Assert
        var versions = await _strategyService.GetStrategyVersionsAsync(1, 1);
        versions.Should().HaveCountGreaterThan(1);
    }

    #endregion

    #region DeleteStrategyAsync Tests

    [Fact]
    public async Task DeleteStrategyAsync_WithValidId_DeletesStrategy()
    {
        // Act
        var result = await _strategyService.DeleteStrategyAsync(1, 1);

        // Assert
        result.Should().BeTrue();

        // Verify deletion
        var strategy = await _strategyService.GetStrategyByIdAsync(1, 1);
        strategy.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStrategyAsync_WithActiveStrategy_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.DeleteStrategyAsync(2, 1); // Strategy 2 is active
        });
    }

    [Fact]
    public async Task DeleteStrategyAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _strategyService.DeleteStrategyAsync(999, 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CloneStrategyAsync Tests

    [Fact]
    public async Task CloneStrategyAsync_WithValidRequest_ClonesStrategy()
    {
        // Arrange
        var request = new CloneStrategyRequest { Name = "Cloned Strategy" };

        // Act
        var result = await _strategyService.CloneStrategyAsync(1, 1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Cloned Strategy");
        result.Description.Should().Be("Test description"); // Should copy description
        result.StrategyType.Should().Be("Custom");
        result.Tags.Should().Be("test,momentum");
        result.Version.Should().Be(1); // New strategy starts at version 1
    }

    [Fact]
    public async Task CloneStrategyAsync_WithDuplicateName_ThrowsException()
    {
        // Arrange
        var request = new CloneStrategyRequest { Name = "Test Strategy 1" }; // Already exists

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.CloneStrategyAsync(1, 1, request);
        });
    }

    [Fact]
    public async Task CloneStrategyAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var request = new CloneStrategyRequest { Name = "Cloned" };

        // Act
        var result = await _strategyService.CloneStrategyAsync(999, 1, request);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Version Management Tests

    [Fact]
    public async Task GetStrategyVersionsAsync_ReturnsVersionHistory()
    {
        // Arrange - Create a strategy and update it to generate versions
        var createRequest = new CreateStrategyRequest
        {
            Name = "Versioned Strategy",
            StrategyType = "Custom"
        };
        var strategy = await _strategyService.CreateStrategyAsync(1, createRequest);

        var updateRequest = new UpdateStrategyRequest
        {
            Description = "Updated description"
        };
        await _strategyService.UpdateStrategyAsync(strategy.Id, 1, updateRequest);

        // Act
        var versions = await _strategyService.GetStrategyVersionsAsync(strategy.Id, 1);

        // Assert
        versions.Should().HaveCount(2); // Initial + update
        versions.Should().BeInDescendingOrder(v => v.VersionNumber);
    }

    [Fact]
    public async Task RollbackToVersionAsync_RestoresPreviousVersion()
    {
        // Arrange - Create and update a strategy
        var createRequest = new CreateStrategyRequest
        {
            Name = "Rollback Test",
            ConfigurationJson = "{\"original\": true}",
            StrategyType = "Custom"
        };
        var strategy = await _strategyService.CreateStrategyAsync(1, createRequest);

        var updateRequest = new UpdateStrategyRequest
        {
            ConfigurationJson = "{\"updated\": true}"
        };
        await _strategyService.UpdateStrategyAsync(strategy.Id, 1, updateRequest);

        var versions = await _strategyService.GetStrategyVersionsAsync(strategy.Id, 1);
        var firstVersion = versions.OrderBy(v => v.VersionNumber).First();

        // Act
        var result = await _strategyService.RollbackToVersionAsync(strategy.Id, firstVersion.Id, 1);

        // Assert
        result.Should().NotBeNull();
        result!.ConfigurationJson.Should().Be("{\"original\": true}");
        result.Version.Should().Be(3); // Original 1 + Update 2 + Rollback 3
    }

    [Fact]
    public async Task RollbackToVersionAsync_WithActiveStrategy_ThrowsException()
    {
        // Arrange
        var versions = await _strategyService.GetStrategyVersionsAsync(2, 1);
        if (versions.Count == 0)
        {
            // Create a version for strategy 2
            var version = new StrategyVersion
            {
                StrategyId = 2,
                VersionNumber = 1,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.GetRepository<StrategyVersion>().AddAsync(version);
            await _unitOfWork.SaveChangesAsync();
            versions = await _strategyService.GetStrategyVersionsAsync(2, 1);
        }

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.RollbackToVersionAsync(2, versions.First().Id, 1);
        });
    }

    #endregion

    #region Tag Management Tests

    [Fact]
    public async Task UpdateStrategyTagsAsync_UpdatesTags()
    {
        // Act
        var result = await _strategyService.UpdateStrategyTagsAsync(1, 1, "updated,tags,momentum");

        // Assert
        result.Should().NotBeNull();
        result!.Tags.Should().Be("updated,tags,momentum");
    }

    [Fact]
    public async Task UpdateStrategyTagsAsync_WithNullTags_ClearsTags()
    {
        // Act
        var result = await _strategyService.UpdateStrategyTagsAsync(1, 1, null);

        // Assert
        result.Should().NotBeNull();
        result!.Tags.Should().BeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetStrategyPerformanceAsync_CalculatesMetricsCorrectly()
    {
        // Arrange - Add more executions with varying returns
        var execution2 = new StrategyExecution
        {
            StrategyId = 1,
            StartedAt = DateTime.UtcNow.AddDays(-2),
            StoppedAt = DateTime.UtcNow.AddDays(-1),
            Status = "Completed",
            ProcessId = 12346,
            TotalReturn = -2.5m,
            OrdersExecuted = 5
        };
        await _unitOfWork.StrategyExecutions.AddAsync(execution2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _strategyService.GetStrategyPerformanceAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result!.StrategyId.Should().Be(1);
        result.CumulativeReturn.Should().Be(3.0m); // 5.5 - 2.5
        result.TotalTrades.Should().Be(15); // 10 + 5
        result.WinningTrades.Should().Be(1);
        result.LosingTrades.Should().Be(1);
        result.WinRate.Should().BeApproximately(50.0m, 0.1m); // 1/2 * 100
    }

    [Fact]
    public async Task GetStrategyPerformanceAsync_WithNoExecutions_ReturnsZeroMetrics()
    {
        // Arrange - Create a strategy with no executions
        var request = new CreateStrategyRequest
        {
            Name = "No Execution Strategy",
            StrategyType = "Custom"
        };
        var strategy = await _strategyService.CreateStrategyAsync(1, request);

        // Act
        var result = await _strategyService.GetStrategyPerformanceAsync(strategy.Id, 1);

        // Assert
        result.Should().NotBeNull();
        result!.CumulativeReturn.Should().Be(0);
        result.TotalTrades.Should().Be(0);
        result.WinRate.Should().Be(0);
    }

    #endregion

    #region Code Upload Tests

    [Fact]
    public async Task UploadStrategyCodeAsync_WithPythonFile_SavesFile()
    {
        // Arrange
        var code = "# Python strategy code\nclass MyStrategy:\n    pass";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(code));

        // Act
        var filePath = await _strategyService.UploadStrategyCodeAsync(1, 1, "strategy.py", stream);

        // Assert
        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();
        
        // Verify strategy was updated
        var strategy = await _strategyService.GetStrategyByIdAsync(1, 1);
        strategy!.CodeFilePath.Should().Be(filePath);
        strategy.Version.Should().Be(2); // Version should increment

        // Cleanup
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    [Fact]
    public async Task UploadStrategyCodeAsync_WithCSharpFile_SavesFile()
    {
        // Arrange
        var code = "using System;\nclass MyStrategy {}";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(code));

        // Act
        var filePath = await _strategyService.UploadStrategyCodeAsync(1, 1, "strategy.cs", stream);

        // Assert
        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();

        // Cleanup
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    [Fact]
    public async Task UploadStrategyCodeAsync_WithInvalidExtension_ThrowsException()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.UploadStrategyCodeAsync(1, 1, "strategy.txt", stream);
        });
    }

    [Fact]
    public async Task UploadStrategyCodeAsync_WithActiveStrategy_ThrowsException()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.UploadStrategyCodeAsync(2, 1, "strategy.py", stream); // Strategy 2 is active
        });
    }

    #endregion

    #region Export/Import Tests

    [Fact]
    public async Task ExportStrategyAsync_CreatesValidZipPackage()
    {
        // Act
        var zipBytes = await _strategyService.ExportStrategyAsync(1, 1);

        // Assert
        zipBytes.Should().NotBeNull();
        zipBytes!.Length.Should().BeGreaterThan(0);

        // Verify ZIP structure
        using var zipStream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        
        archive.Entries.Should().Contain(e => e.Name == "strategy.json");
        archive.Entries.Should().Contain(e => e.Name == "README.md");
    }

    [Fact]
    public async Task ImportStrategyAsync_WithValidPackage_CreatesStrategy()
    {
        // Arrange - Create a valid ZIP package
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var metadataEntry = archive.CreateEntry("strategy.json");
            using (var entryStream = metadataEntry.Open())
            using (var writer = new StreamWriter(entryStream))
            {
                var metadata = new
                {
                    Name = "Imported Strategy",
                    Description = "Imported description",
                    StrategyType = "Custom",
                    Tags = "imported,test"
                };
                await writer.WriteAsync(JsonSerializer.Serialize(metadata));
            }
        }
        memoryStream.Position = 0;

        // Act
        var result = await _strategyService.ImportStrategyAsync(1, memoryStream);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Imported Strategy");
        result.Description.Should().Be("Imported description");
        result.Tags.Should().Be("imported,test");
    }

    [Fact]
    public async Task ImportStrategyAsync_WithDuplicateName_AppendsCounter()
    {
        // Arrange - Import twice with same name
        var createZip = () =>
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("strategy.json");
                using (var entryStream = entry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    var metadata = new { Name = "Duplicate Import", StrategyType = "Custom" };
                    writer.Write(JsonSerializer.Serialize(metadata));
                }
            }
            stream.Position = 0;
            return stream;
        };

        // Act
        var result1 = await _strategyService.ImportStrategyAsync(1, createZip());
        var result2 = await _strategyService.ImportStrategyAsync(1, createZip());

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.Name.Should().Be("Duplicate Import");
        result2!.Name.Should().Be("Duplicate Import (1)");
    }

    [Fact]
    public async Task ImportStrategyAsync_WithMissingMetadata_ThrowsException()
    {
        // Arrange - Create ZIP without strategy.json
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("README.md");
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            await writer.WriteAsync("test");
        }
        memoryStream.Position = 0;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.ImportStrategyAsync(1, memoryStream);
        });
    }

    #endregion

    #region Lean Config Generation Tests

    [Fact]
    public async Task GenerateLeanConfigAsync_GeneratesValidJson()
    {
        // Act
        var configJson = await _strategyService.GenerateLeanConfigAsync(1, 1);

        // Assert
        configJson.Should().NotBeNullOrEmpty();
        
        // Verify it's valid JSON
        var config = JsonSerializer.Deserialize<JsonElement>(configJson);
        config.GetProperty("algorithm_type_name").GetString().Should().Be("Test Strategy 1");
        config.GetProperty("strategy_id").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GenerateLeanConfigAsync_WithPythonCode_SetsCorrectLanguage()
    {
        // Arrange - Upload Python code
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("# Python code"));
        var filePath = await _strategyService.UploadStrategyCodeAsync(1, 1, "strategy.py", stream);

        // Act
        var configJson = await _strategyService.GenerateLeanConfigAsync(1, 1);

        // Assert
        var config = JsonSerializer.Deserialize<JsonElement>(configJson);
        config.GetProperty("algorithm_language").GetString().Should().Be("Python");

        // Cleanup
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    [Fact]
    public async Task GenerateLeanConfigAsync_WithInvalidId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _strategyService.GenerateLeanConfigAsync(999, 1);
        });
    }

    #endregion
}
