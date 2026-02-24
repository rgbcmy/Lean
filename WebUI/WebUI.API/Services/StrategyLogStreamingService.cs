using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using WebUI.API.Hubs;
using WebUI.Data.Services;

namespace WebUI.API.Services;

/// <summary>
/// Strategy log streaming service interface / 策略日志流式传输服务接口
/// </summary>
public interface IStrategyLogStreamingService : IStrategyLogStreamer
{
    /// <summary>
    /// Check if streaming is active for a strategy / 检查是否正在流式传输
    /// </summary>
    bool IsStreaming(int strategyId);
}

/// <summary>
/// Strategy log streaming service implementation / 策略日志流式传输服务实现
/// Watches strategy log files and pushes updates to SignalR clients
/// </summary>
public class StrategyLogStreamingService : IStrategyLogStreamingService, IDisposable
{
    private readonly IHubContext<StrategyHub> _hubContext;
    private readonly ILogger<StrategyLogStreamingService> _logger;
    
    // Track active file watchers: strategyId -> (watcher, cancellationTokenSource)
    private readonly ConcurrentDictionary<int, (FileSystemWatcher watcher, CancellationTokenSource cts)> _watchers = new();
    
    // Track log file positions to avoid re-reading
    private readonly ConcurrentDictionary<int, long> _filePositions = new();

    public StrategyLogStreamingService(
        IHubContext<StrategyHub> hubContext,
        ILogger<StrategyLogStreamingService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task StartStreamingAsync(int strategyId, string logFilePath)
    {
        if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
        {
            _logger.LogWarning("Cannot start streaming for strategy {StrategyId}, log file not found: {LogFilePath}", 
                strategyId, logFilePath);
            return;
        }

        if (_watchers.ContainsKey(strategyId))
        {
            _logger.LogInformation("Already streaming logs for strategy {StrategyId}", strategyId);
            return;
        }

        _logger.LogInformation("Starting log streaming for strategy {StrategyId}: {LogFilePath}", 
            strategyId, logFilePath);

        var directory = Path.GetDirectoryName(logFilePath);
        var fileName = Path.GetFileName(logFilePath);

        if (string.IsNullOrEmpty(directory))
        {
            directory = ".";
        }

        var watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        var cts = new CancellationTokenSource();

        watcher.Changed += async (sender, e) =>
        {
            await OnLogFileChanged(strategyId, logFilePath, cts.Token);
        };

        _watchers[strategyId] = (watcher, cts);
        _filePositions[strategyId] = 0;

        // Send initial log content
        await SendInitialLogContentAsync(strategyId, logFilePath, cts.Token);
    }

    public Task StopStreamingAsync(int strategyId)
    {
        if (_watchers.TryRemove(strategyId, out var watcherInfo))
        {
            _logger.LogInformation("Stopping log streaming for strategy {StrategyId}", strategyId);
            
            var (watcher, cts) = watcherInfo;
            
            cts.Cancel();
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            cts.Dispose();

            _filePositions.TryRemove(strategyId, out _);
        }

        return Task.CompletedTask;
    }

    public bool IsStreaming(int strategyId)
    {
        return _watchers.ContainsKey(strategyId);
    }

    private async Task SendInitialLogContentAsync(int strategyId, string logFilePath, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(logFilePath))
            {
                return;
            }

            // Read last N lines (max 100) to avoid overwhelming clients
            const int maxInitialLines = 100;
            var lines = await ReadLastLinesAsync(logFilePath, maxInitialLines, cancellationToken);

            if (lines.Count > 0)
            {
                // Update file position
                using var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                _filePositions[strategyId] = fs.Length;

                // Send to SignalR clients subscribed to this strategy
                var groupName = $"StrategyLog:{strategyId}";
                
                foreach (var line in lines)
                {
                    var logEntry = ParseLogLine(line);
                    await _hubContext.Clients.Group(groupName).SendAsync("StrategyLogUpdate", new
                    {
                        strategyId,
                        timestamp = DateTime.UtcNow,
                        level = logEntry.level,
                        message = logEntry.message,
                        isInitial = true
                    }, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending initial log content for strategy {StrategyId}", strategyId);
        }
    }

    private async Task OnLogFileChanged(int strategyId, string logFilePath, CancellationToken cancellationToken)
    {
        try
        {
            if (!_filePositions.TryGetValue(strategyId, out var lastPosition))
            {
                lastPosition = 0;
            }

            using var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            
            // Check if file was truncated or rotated
            if (fs.Length < lastPosition)
            {
                lastPosition = 0;
            }

            if (fs.Length == lastPosition)
            {
                return; // No new content
            }

            // Seek to last read position
            fs.Seek(lastPosition, SeekOrigin.Begin);

            using var reader = new StreamReader(fs);
            
            var groupName = $"StrategyLog:{strategyId}";
            var newLines = new List<string>();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    newLines.Add(line);
                }
            }

            // Update position
            _filePositions[strategyId] = fs.Position;

            // Send new lines to SignalR clients
            foreach (var logLine in newLines)
            {
                var logEntry = ParseLogLine(logLine);
                await _hubContext.Clients.Group(groupName).SendAsync("StrategyLogUpdate", new
                {
                    strategyId,
                    timestamp = DateTime.UtcNow,
                    level = logEntry.level,
                    message = logEntry.message,
                    isInitial = false
                }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing log file changes for strategy {StrategyId}", strategyId);
        }
    }

    private async Task<List<string>> ReadLastLinesAsync(string filePath, int maxLines, CancellationToken cancellationToken)
    {
        var lines = new List<string>();

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            
            var allLines = new List<string>();
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    allLines.Add(line);
                }
            }

            // Take last N lines
            lines = allLines.Skip(Math.Max(0, allLines.Count - maxLines)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading log file: {FilePath}", filePath);
        }

        return lines;
    }

    private (string level, string message) ParseLogLine(string line)
    {
        // Simple log parsing - assumes format like: "[ERROR] message" or "timestamp [INFO] message"
        // Adjust based on actual Lean log format
        
        var level = "INFO";
        var message = line;

        if (line.Contains("[ERROR]") || line.Contains("[ERR]"))
        {
            level = "ERROR";
        }
        else if (line.Contains("[WARN]") || line.Contains("[WARNING]"))
        {
            level = "WARN";
        }
        else if (line.Contains("[DEBUG]"))
        {
            level = "DEBUG";
        }
        else if (line.Contains("[TRACE]"))
        {
            level = "TRACE";
        }

        return (level, message);
    }

    public void Dispose()
    {
        foreach (var kvp in _watchers.ToArray())
        {
            StopStreamingAsync(kvp.Key).Wait();
        }
    }
}
