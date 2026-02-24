using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WebUI.Core.Models;
using WebUI.Data.Entities;
using System.Text.Json;
using System.IO.Compression;
using System.Text;

namespace WebUI.Data.Services;

/// <summary>
/// Strategy management service implementation / 策略管理服务实现
/// </summary>
public class StrategyService : IStrategyService
{
    private readonly ILogger<StrategyService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _strategiesDirectory;

    public StrategyService(
        ILogger<StrategyService> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        
        // Create Strategies directory if it doesn't exist
        _strategiesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Strategies");
        if (!Directory.Exists(_strategiesDirectory))
        {
            Directory.CreateDirectory(_strategiesDirectory);
        }
    }

    public async Task<StrategyListResponse> GetStrategiesAsync(
        int userId,
        string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var strategies = await _unitOfWork.Strategies.GetByUserIdAsync(userId, cancellationToken);
        
        // Apply status filter
        if (!string.IsNullOrEmpty(statusFilter))
        {
            strategies = statusFilter.ToLowerInvariant() switch
            {
                "active" => strategies.Where(s => s.IsActive),
                "inactive" => strategies.Where(s => !s.IsActive),
                _ => strategies
            };
        }

        var strategyList = new List<StrategyDto>();
        foreach (var strategy in strategies)
        {
            // Get last execution for this strategy
            var lastExecution = await _unitOfWork.StrategyExecutions
                .FindAsync(e => e.StrategyId == strategy.Id, cancellationToken);
            var lastRun = lastExecution.OrderByDescending(e => e.StartedAt).FirstOrDefault();

            strategyList.Add(MapToDto(strategy, lastRun));
        }

        _logger.LogInformation("Retrieved {Count} strategies for user {UserId}", strategyList.Count, userId);

        return new StrategyListResponse
        {
            Strategies = strategyList,
            TotalCount = strategyList.Count
        };
    }

    public async Task<StrategyDetailDto?> GetStrategyByIdAsync(
        int strategyId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            return null;
        }

        // Get recent executions
        var executions = await _unitOfWork.StrategyExecutions
            .FindAsync(e => e.StrategyId == strategyId, cancellationToken);
        
        var recentExecutions = executions
            .OrderByDescending(e => e.StartedAt)
            .Take(10)
            .Select(MapToExecutionSummary)
            .ToList();

        return MapToDetailDto(strategy, recentExecutions);
    }

    public async Task<StrategyDetailDto> CreateStrategyAsync(
        int userId,
        CreateStrategyRequest request,
        CancellationToken cancellationToken = default)
    {
        // Check if name already exists for this user
        var existingStrategy = await _unitOfWork.Strategies.GetByNameAsync(request.Name, cancellationToken);
        if (existingStrategy != null && existingStrategy.UserId == userId)
        {
            throw new InvalidOperationException($"Strategy with name '{request.Name}' already exists / 策略名称 '{request.Name}' 已存在");
        }

        // Create strategy entity
        var strategy = new Strategy
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            StrategyType = request.StrategyType,
            ConfigurationJson = request.ConfigurationJson,
            Tags = request.Tags,
            IsActive = false,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Strategies.AddAsync(strategy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create initial version
        await CreateVersionSnapshotAsync(strategy, "Initial version / 初始版本", cancellationToken);

        _logger.LogInformation("Created strategy {StrategyId} '{Name}' for user {UserId}", 
            strategy.Id, strategy.Name, userId);

        return MapToDetailDto(strategy, new List<StrategyExecutionSummary>());
    }

    public async Task<StrategyDetailDto?> UpdateStrategyAsync(
        int strategyId,
        int userId,
        UpdateStrategyRequest request,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            return null;
        }

        // Prevent editing if strategy is active
        if (strategy.IsActive)
        {
            throw new InvalidOperationException("Cannot edit an active strategy. Please stop it first / 无法编辑运行中的策略，请先停止它");
        }

        bool hasChanges = false;

        if (!string.IsNullOrEmpty(request.Name) && request.Name != strategy.Name)
        {
            // Check for name conflicts
            var existingStrategy = await _unitOfWork.Strategies.GetByNameAsync(request.Name, cancellationToken);
            if (existingStrategy != null && existingStrategy.Id != strategyId && existingStrategy.UserId == userId)
            {
                throw new InvalidOperationException($"Strategy with name '{request.Name}' already exists / 策略名称 '{request.Name}' 已存在");
            }
            strategy.Name = request.Name;
            hasChanges = true;
        }

        if (request.Description != null && request.Description != strategy.Description)
        {
            strategy.Description = request.Description;
            hasChanges = true;
        }

        if (request.ConfigurationJson != null && request.ConfigurationJson != strategy.ConfigurationJson)
        {
            strategy.ConfigurationJson = request.ConfigurationJson;
            hasChanges = true;
        }

        if (request.Tags != null && request.Tags != strategy.Tags)
        {
            strategy.Tags = request.Tags;
            hasChanges = true;
        }

        if (hasChanges)
        {
            strategy.Version++;
            strategy.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Strategies.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create version snapshot
            await CreateVersionSnapshotAsync(strategy, $"Updated to version {strategy.Version}", cancellationToken);

            _logger.LogInformation("Updated strategy {StrategyId} to version {Version}", 
                strategy.Id, strategy.Version);
        }

        return await GetStrategyByIdAsync(strategyId, userId, cancellationToken);
    }

    public async Task<bool> DeleteStrategyAsync(
        int strategyId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            return false;
        }

        // Prevent deleting if strategy is active
        if (strategy.IsActive)
        {
            throw new InvalidOperationException("Cannot delete an active strategy. Please stop it first / 无法删除运行中的策略，请先停止它");
        }

        // Delete code files if they exist
        if (!string.IsNullOrEmpty(strategy.CodeFilePath) && File.Exists(strategy.CodeFilePath))
        {
            try
            {
                File.Delete(strategy.CodeFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete strategy code file: {FilePath}", strategy.CodeFilePath);
            }
        }

        _unitOfWork.Strategies.Remove(strategy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted strategy {StrategyId} '{Name}'", strategyId, strategy.Name);

        return true;
    }

    public async Task<StrategyDetailDto?> CloneStrategyAsync(
        int strategyId,
        int userId,
        CloneStrategyRequest request,
        CancellationToken cancellationToken = default)
    {
        var sourceStrategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (sourceStrategy == null || sourceStrategy.UserId != userId)
        {
            return null;
        }

        // Check if name already exists
        var existingStrategy = await _unitOfWork.Strategies.GetByNameAsync(request.Name, cancellationToken);
        if (existingStrategy != null && existingStrategy.UserId == userId)
        {
            throw new InvalidOperationException($"Strategy with name '{request.Name}' already exists / 策略名称 '{request.Name}' 已存在");
        }

        // Create cloned strategy
        var clonedStrategy = new Strategy
        {
            UserId = userId,
            Name = request.Name,
            Description = sourceStrategy.Description,
            StrategyType = sourceStrategy.StrategyType,
            ConfigurationJson = sourceStrategy.ConfigurationJson,
            Tags = sourceStrategy.Tags,
            IsActive = false,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Clone code file if it exists
        if (!string.IsNullOrEmpty(sourceStrategy.CodeFilePath) && File.Exists(sourceStrategy.CodeFilePath))
        {
            var sourceFileName = Path.GetFileName(sourceStrategy.CodeFilePath);
            var extension = Path.GetExtension(sourceFileName);
            var newFileName = $"{SanitizeFileName(request.Name)}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var newFilePath = Path.Combine(_strategiesDirectory, newFileName);

            File.Copy(sourceStrategy.CodeFilePath, newFilePath);
            clonedStrategy.CodeFilePath = newFilePath;
        }

        await _unitOfWork.Strategies.AddAsync(clonedStrategy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create initial version for cloned strategy
        await CreateVersionSnapshotAsync(clonedStrategy, $"Cloned from '{sourceStrategy.Name}' / 从 '{sourceStrategy.Name}' 克隆", cancellationToken);

        _logger.LogInformation("Cloned strategy {SourceId} to {ClonedId} with name '{Name}'", 
            strategyId, clonedStrategy.Id, request.Name);

        return MapToDetailDto(clonedStrategy, new List<StrategyExecutionSummary>());
    }

    public async Task<List<StrategyVersionDto>> GetStrategyVersionsAsync(
        int strategyId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            return new List<StrategyVersionDto>();
        }

        var versions = await _unitOfWork.GetRepository<StrategyVersion>()
            .FindAsync(v => v.StrategyId == strategyId, cancellationToken);

        return versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new StrategyVersionDto
            {
                Id = v.Id,
                StrategyId = v.StrategyId,
                VersionNumber = v.VersionNumber,
                CreatedAt = v.CreatedAt,
                ChangeDescription = v.ChangeDescription,
                ConfigurationJson = v.ConfigurationJson,
                CodeFilePath = v.CodeFilePath
            })
            .ToList();
    }

    public async Task<StrategyDetailDto?> RollbackToVersionAsync(
        int strategyId,
        int versionId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            return null;
        }

        // Prevent rollback if strategy is active
        if (strategy.IsActive)
        {
            throw new InvalidOperationException("Cannot rollback an active strategy. Please stop it first / 无法回滚运行中的策略，请先停止它");
        }

        var version = await _unitOfWork.GetRepository<StrategyVersion>()
            .GetByIdAsync(versionId, cancellationToken);

        if (version == null || version.StrategyId != strategyId)
        {
            return null;
        }

        // Restore from version
        strategy.ConfigurationJson = version.ConfigurationJson;
        strategy.CodeFilePath = version.CodeFilePath;
        strategy.Version++;
        strategy.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Strategies.Update(strategy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create new version snapshot
        await CreateVersionSnapshotAsync(strategy, $"Rolled back to version {version.VersionNumber} / 回滚到版本 {version.VersionNumber}", cancellationToken);

        _logger.LogInformation("Rolled back strategy {StrategyId} to version {VersionNumber}", 
            strategyId, version.VersionNumber);

        return await GetStrategyByIdAsync(strategyId, userId, cancellationToken);
    }

    public async Task<StrategyDetailDto?> UpdateStrategyTagsAsync(
        int strategyId,
        int userId,
        string? tags,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            return null;
        }

        strategy.Tags = tags;
        strategy.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Strategies.Update(strategy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated tags for strategy {StrategyId}", strategyId);

        return await GetStrategyByIdAsync(strategyId, userId, cancellationToken);
    }

    public async Task<StrategyPerformanceDto?> GetStrategyPerformanceAsync(
        int strategyId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            return null;
        }

        // Get all completed executions
        var executions = await _unitOfWork.StrategyExecutions
            .FindAsync(e => e.StrategyId == strategyId && e.Status == "Completed", cancellationToken);

        var completedExecutions = executions.ToList();

        if (completedExecutions.Count == 0)
        {
            return new StrategyPerformanceDto
            {
                StrategyId = strategyId,
                StrategyName = strategy.Name,
                CumulativeReturn = 0,
                WinRate = 0,
                MaxDrawdown = 0,
                TotalTrades = 0,
                WinningTrades = 0,
                LosingTrades = 0
            };
        }

        // Calculate performance metrics
        var totalReturn = completedExecutions
            .Where(e => e.TotalReturn.HasValue)
            .Sum(e => e.TotalReturn!.Value);

        var totalTrades = completedExecutions.Sum(e => e.OrdersExecuted);
        var winningTrades = completedExecutions.Count(e => e.TotalReturn.HasValue && e.TotalReturn.Value > 0);
        var losingTrades = completedExecutions.Count(e => e.TotalReturn.HasValue && e.TotalReturn.Value < 0);
        var winRate = totalTrades > 0 ? (decimal)winningTrades / totalTrades * 100 : 0;

        // Calculate average win/loss
        var wins = completedExecutions
            .Where(e => e.TotalReturn.HasValue && e.TotalReturn.Value > 0)
            .Select(e => e.TotalReturn!.Value)
            .ToList();
        var losses = completedExecutions
            .Where(e => e.TotalReturn.HasValue && e.TotalReturn.Value < 0)
            .Select(e => e.TotalReturn!.Value)
            .ToList();

        var avgWin = wins.Count > 0 ? wins.Average() : (decimal?)null;
        var avgLoss = losses.Count > 0 ? losses.Average() : (decimal?)null;

        // Calculate profit factor
        var totalGain = wins.Sum();
        var totalLoss = Math.Abs(losses.Sum());
        var profitFactor = totalLoss > 0 ? totalGain / totalLoss : (decimal?)null;

        // TODO: Calculate Sharpe ratio and max drawdown from equity curve
        // For now, return placeholder values

        return new StrategyPerformanceDto
        {
            StrategyId = strategyId,
            StrategyName = strategy.Name,
            CumulativeReturn = totalReturn,
            WinRate = winRate,
            SharpeRatio = null, // TODO: Calculate from equity curve
            MaxDrawdown = 0, // TODO: Calculate from equity curve
            TotalTrades = totalTrades,
            WinningTrades = winningTrades,
            LosingTrades = losingTrades,
            AverageWin = avgWin,
            AverageLoss = avgLoss,
            ProfitFactor = profitFactor
        };
    }

    public async Task<string> UploadStrategyCodeAsync(
        int strategyId,
        int userId,
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            throw new InvalidOperationException("Strategy not found / 策略不存在");
        }

        // Prevent upload if strategy is active
        if (strategy.IsActive)
        {
            throw new InvalidOperationException("Cannot upload code to an active strategy. Please stop it first / 无法上传代码到运行中的策略，请先停止它");
        }

        // Validate file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension != ".py" && extension != ".cs")
        {
            throw new InvalidOperationException("Only .py and .cs files are supported / 仅支持 .py 和 .cs 文件");
        }

        // Delete old code file if exists
        if (!string.IsNullOrEmpty(strategy.CodeFilePath) && File.Exists(strategy.CodeFilePath))
        {
            File.Delete(strategy.CodeFilePath);
        }

        // Generate unique filename
        var sanitizedName = SanitizeFileName(strategy.Name);
        var newFileName = $"{sanitizedName}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine(_strategiesDirectory, newFileName);

        // Save file
        using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOut, cancellationToken);
        }

        // Update strategy
        strategy.CodeFilePath = filePath;
        strategy.Version++;
        strategy.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Strategies.Update(strategy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create version snapshot
        await CreateVersionSnapshotAsync(strategy, $"Uploaded code file: {fileName}", cancellationToken);

        _logger.LogInformation("Uploaded code file for strategy {StrategyId}: {FilePath}", strategyId, filePath);

        return filePath;
    }

    public async Task<byte[]?> ExportStrategyAsync(
        int strategyId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Add strategy metadata
            var metadataEntry = archive.CreateEntry("strategy.json");
            using (var entryStream = metadataEntry.Open())
            using (var writer = new StreamWriter(entryStream))
            {
                var metadata = new
                {
                    strategy.Name,
                    strategy.Description,
                    strategy.StrategyType,
                    strategy.ConfigurationJson,
                    strategy.Tags,
                    ExportedAt = DateTime.UtcNow
                };
                await writer.WriteAsync(JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
            }

            // Add code file if exists
            if (!string.IsNullOrEmpty(strategy.CodeFilePath) && File.Exists(strategy.CodeFilePath))
            {
                var codeFileName = Path.GetFileName(strategy.CodeFilePath);
                var codeEntry = archive.CreateEntry($"code/{codeFileName}");
                using (var entryStream = codeEntry.Open())
                using (var codeFileStream = File.OpenRead(strategy.CodeFilePath))
                {
                    await codeFileStream.CopyToAsync(entryStream, cancellationToken);
                }
            }

            // Add README
            var readmeEntry = archive.CreateEntry("README.md");
            using (var entryStream = readmeEntry.Open())
            using (var writer = new StreamWriter(entryStream))
            {
                await writer.WriteAsync($"# {strategy.Name}\n\n{strategy.Description}\n\n" +
                    $"Type: {strategy.StrategyType}\n" +
                    $"Tags: {strategy.Tags}\n" +
                    $"Version: {strategy.Version}\n" +
                    $"Exported: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n");
            }
        }

        _logger.LogInformation("Exported strategy {StrategyId} '{Name}'", strategyId, strategy.Name);

        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    public async Task<StrategyDetailDto?> ImportStrategyAsync(
        int userId,
        Stream zipStream,
        CancellationToken cancellationToken = default)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        // Read strategy metadata
        var metadataEntry = archive.GetEntry("strategy.json");
        if (metadataEntry == null)
        {
            throw new InvalidOperationException("Invalid strategy package: missing strategy.json / 无效的策略包：缺少 strategy.json");
        }

        using var metadataStream = metadataEntry.Open();
        using var reader = new StreamReader(metadataStream);
        var metadataJson = await reader.ReadToEndAsync();
        var metadata = JsonSerializer.Deserialize<JsonElement>(metadataJson);

        var name = metadata.GetProperty("Name").GetString() ?? "Imported Strategy";
        
        // Ensure unique name
        var baseName = name;
        var counter = 1;
        while (true)
        {
            var existing = await _unitOfWork.Strategies.GetByNameAsync(name, cancellationToken);
            if (existing == null || existing.UserId != userId)
                break;
            name = $"{baseName} ({counter++})";
        }

        // Create strategy
        var strategy = new Strategy
        {
            UserId = userId,
            Name = name,
            Description = metadata.TryGetProperty("Description", out var desc) ? desc.GetString() : null,
            StrategyType = metadata.TryGetProperty("StrategyType", out var type) ? type.GetString() ?? "Custom" : "Custom",
            ConfigurationJson = metadata.TryGetProperty("ConfigurationJson", out var config) ? config.GetString() : null,
            Tags = metadata.TryGetProperty("Tags", out var tags) ? tags.GetString() : null,
            IsActive = false,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Extract code files
        var codeEntry = archive.Entries.FirstOrDefault(e => e.FullName.StartsWith("code/"));
        if (codeEntry != null)
        {
            var extension = Path.GetExtension(codeEntry.Name);
            var newFileName = $"{SanitizeFileName(strategy.Name)}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(_strategiesDirectory, newFileName);

            using (var entryStream = codeEntry.Open())
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await entryStream.CopyToAsync(fileStream, cancellationToken);
            }

            strategy.CodeFilePath = filePath;
        }

        await _unitOfWork.Strategies.AddAsync(strategy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create initial version
        await CreateVersionSnapshotAsync(strategy, "Imported strategy / 导入策略", cancellationToken);

        _logger.LogInformation("Imported strategy {StrategyId} '{Name}' for user {UserId}", 
            strategy.Id, strategy.Name, userId);

        return MapToDetailDto(strategy, new List<StrategyExecutionSummary>());
    }

    public async Task<string> GenerateLeanConfigAsync(
        int strategyId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var strategy = await _unitOfWork.Strategies.GetByIdAsync(strategyId, cancellationToken);
        
        if (strategy == null || strategy.UserId != userId)
        {
            throw new InvalidOperationException("Strategy not found / 策略不存在");
        }

        // Generate Lean configuration JSON
        var config = new
        {
            algorithm_type_name = strategy.Name,
            algorithm_language = strategy.CodeFilePath?.EndsWith(".py") == true ? "Python" : "CSharp",
            algorithm_location = strategy.CodeFilePath,
            data_folder = "../../../Data/",
            environment = "live-paper",
            live_mode_brokerage = "InteractiveBrokersBrokerage",
            strategy_id = strategy.Id,
            configuration = !string.IsNullOrEmpty(strategy.ConfigurationJson) 
                ? JsonSerializer.Deserialize<JsonElement>(strategy.ConfigurationJson)
                : (object?)null
        };

        var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        _logger.LogInformation("Generated Lean config for strategy {StrategyId}", strategyId);

        return configJson;
    }

    // Helper methods
    private async Task CreateVersionSnapshotAsync(
        Strategy strategy,
        string? changeDescription,
        CancellationToken cancellationToken)
    {
        var version = new StrategyVersion
        {
            StrategyId = strategy.Id,
            VersionNumber = strategy.Version,
            CreatedAt = DateTime.UtcNow,
            ChangeDescription = changeDescription,
            ConfigurationJson = strategy.ConfigurationJson,
            CodeFilePath = strategy.CodeFilePath
        };

        await _unitOfWork.GetRepository<StrategyVersion>().AddAsync(version, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static StrategyDto MapToDto(Strategy strategy, StrategyExecution? lastExecution)
    {
        return new StrategyDto
        {
            Id = strategy.Id,
            Name = strategy.Name,
            Description = strategy.Description,
            StrategyType = strategy.StrategyType,
            Tags = strategy.Tags,
            IsActive = strategy.IsActive,
            Version = strategy.Version,
            CreatedAt = strategy.CreatedAt,
            UpdatedAt = strategy.UpdatedAt,
            LastRunTime = lastExecution?.StartedAt,
            CumulativeReturn = lastExecution?.TotalReturn
        };
    }

    private static StrategyDetailDto MapToDetailDto(Strategy strategy, List<StrategyExecutionSummary> recentExecutions)
    {
        return new StrategyDetailDto
        {
            Id = strategy.Id,
            Name = strategy.Name,
            Description = strategy.Description,
            StrategyType = strategy.StrategyType,
            ConfigurationJson = strategy.ConfigurationJson,
            CodeFilePath = strategy.CodeFilePath,
            Tags = strategy.Tags,
            IsActive = strategy.IsActive,
            Version = strategy.Version,
            CreatedAt = strategy.CreatedAt,
            UpdatedAt = strategy.UpdatedAt,
            RecentExecutions = recentExecutions
        };
    }

    private static StrategyExecutionSummary MapToExecutionSummary(StrategyExecution execution)
    {
        return new StrategyExecutionSummary
        {
            Id = execution.Id,
            StartedAt = execution.StartedAt,
            StoppedAt = execution.StoppedAt,
            Status = execution.Status,
            TotalReturn = execution.TotalReturn,
            OrdersExecuted = execution.OrdersExecuted,
            ErrorMessage = execution.ErrorMessage
        };
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }
}
