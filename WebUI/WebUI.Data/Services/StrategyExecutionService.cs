using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Data.Entities;

namespace WebUI.Data.Services;

/// <summary>
/// Simple interface for log streaming to avoid circular dependency
/// </summary>
public interface IStrategyLogStreamer
{
    Task StartStreamingAsync(int strategyId, string logFilePath);
    Task StopStreamingAsync(int strategyId);
}

/// <summary>
/// Strategy execution service implementation / 策略执行服务实现
/// Manages Lean process lifecycle, health monitoring, and scheduling
/// </summary>
public class StrategyExecutionService : IStrategyExecutionService
{
    private readonly WebUIDbContext _context;
    private readonly ILogger<StrategyExecutionService> _logger;
    private readonly IStrategyService _strategyService;
    private IStrategyLogStreamer? _logStreamingService;
    
    // Track running processes: strategyId -> (process, execution)
    private readonly ConcurrentDictionary<int, (Process process, StrategyExecution execution)> _runningProcesses = new();
    
    // Track auto-restart flags: strategyId -> autoRestart
    private readonly ConcurrentDictionary<int, bool> _autoRestartFlags = new();
    
    // Track process resource usage for monitoring
    private readonly ConcurrentDictionary<int, (double cpu, long memoryMB, DateTime measured)> _resourceUsage = new();

    private const int MaxGracefulShutdownSeconds = 30;
    private const int HealthCheckIntervalSeconds = 10;
    private const int AutoRestartDelaySeconds = 5;

    public StrategyExecutionService(
        WebUIDbContext context,
        ILogger<StrategyExecutionService> logger,
        IStrategyService strategyService)
    {
        _context = context;
        _logger = logger;
        _strategyService = strategyService;
        
        // Start background health monitoring
        _ = StartHealthMonitoringLoop();
    }

    /// <summary>
    /// Set log streaming service (optional, to avoid circular dependency)
    /// </summary>
    public void SetLogStreamingService(IStrategyLogStreamer logStreamingService)
    {
        _logStreamingService = logStreamingService;
    }

    public async Task<StrategyExecutionDetailDto> StartStrategyAsync(
        int strategyId, 
        int userId, 
        StartStrategyRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting strategy {StrategyId} for user {UserId}", strategyId, userId);

        // Check if strategy exists and belongs to user
        var strategy = await _context.Strategies
            .FirstOrDefaultAsync(s => s.Id == strategyId && s.UserId == userId, cancellationToken);

        if (strategy == null)
        {
            throw new InvalidOperationException("Strategy not found / 策略不存在");
        }

        // Check if already running
        if (_runningProcesses.ContainsKey(strategyId))
        {
            throw new InvalidOperationException("Strategy is already running / 策略正在运行中，请先停止");
        }

        // Validate strategy config
        if (string.IsNullOrEmpty(strategy.ConfigurationJson))
        {
            throw new InvalidOperationException("Strategy configuration is missing / 策略配置缺失");
        }

        // Generate Lean configuration with parameter overrides
        var configJson = await _strategyService.GenerateLeanConfigAsync(strategyId, userId, cancellationToken);
        
        if (!string.IsNullOrEmpty(request.ParameterOverrides))
        {
            configJson = MergeConfigWithOverrides(configJson, request.ParameterOverrides);
        }

        // Create execution record
        var execution = new StrategyExecution
        {
            StrategyId = strategyId,
            BrokerAccountId = request.BrokerAccountId,
            StartedAt = DateTime.UtcNow,
            Status = "Running",
            ProcessId = 0, // Will be set after process starts
            InitialCapital = ExtractInitialCapital(configJson)
        };

        _context.StrategyExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            // Start Lean process
            var process = await StartLeanProcessAsync(strategyId, configJson, execution.Id);
            
            execution.ProcessId = process.Id;
            await _context.SaveChangesAsync(cancellationToken);

            // Track running process
            _runningProcesses[strategyId] = (process, execution);
            _autoRestartFlags[strategyId] = request.AutoRestart;

            // Update strategy status
            strategy.IsActive = true;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully started strategy {StrategyId} with process {ProcessId}", 
                strategyId, process.Id);

            // Set up process exit handler
            process.Exited += (sender, args) => OnProcessExited(strategyId, process.ExitCode);

            return MapToExecutionDetail(execution, strategy.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start strategy {StrategyId}", strategyId);
            
            execution.Status = "Failed";
            execution.StoppedAt = DateTime.UtcNow;
            execution.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException($"Failed to start strategy: {ex.Message} / 启动策略失败：{ex.Message}");
        }
    }

    public async Task StopStrategyAsync(
        int strategyId, 
        int userId, 
        StopStrategyRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping strategy {StrategyId} for user {UserId}", strategyId, userId);

        // Verify ownership
        var strategy = await _context.Strategies
            .FirstOrDefaultAsync(s => s.Id == strategyId && s.UserId == userId, cancellationToken);

        if (strategy == null)
        {
            throw new InvalidOperationException("Strategy not found / 策略不存在");
        }

        if (!_runningProcesses.TryGetValue(strategyId, out var runningInfo))
        {
            throw new InvalidOperationException("Strategy is not running / 策略未运行");
        }

        var (process, execution) = runningInfo;

        try
        {
            // Disable auto-restart during manual stop
            _autoRestartFlags[strategyId] = false;

            // Try graceful shutdown first
            if (!request.ForceKill)
            {
                _logger.LogInformation("Attempting graceful shutdown of strategy {StrategyId}", strategyId);
                
                process.CloseMainWindow();
                
                var gracefulTimeout = request.TimeoutSeconds > 0 ? request.TimeoutSeconds : MaxGracefulShutdownSeconds;
                var shutdownTask = Task.Run(() => process.WaitForExit(gracefulTimeout * 1000));
                var completed = await Task.WhenAny(shutdownTask, Task.Delay(gracefulTimeout * 1000, cancellationToken)) == shutdownTask;

                if (!completed || !process.HasExited)
                {
                    _logger.LogWarning("Strategy {StrategyId} did not respond to graceful shutdown, forcing kill", strategyId);
                    process.Kill(entireProcessTree: true);
                }
            }
            else
            {
                _logger.LogWarning("Force killing strategy {StrategyId}", strategyId);
                process.Kill(entireProcessTree: true);
            }

            // Wait for process to exit
            await Task.Run(() => process.WaitForExit(5000), cancellationToken);

            // Update execution record
            execution.Status = "Stopped";
            execution.StoppedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Clean up tracking
            _runningProcesses.TryRemove(strategyId, out _);
            _autoRestartFlags.TryRemove(strategyId, out _);
            _resourceUsage.TryRemove(strategyId, out _);

            // Stop log streaming (if available)
            if (_logStreamingService != null)
            {
                await _logStreamingService.StopStreamingAsync(strategyId);
            }

            // Update strategy status
            strategy.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully stopped strategy {StrategyId}", strategyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping strategy {StrategyId}", strategyId);
            throw new InvalidOperationException($"Failed to stop strategy: {ex.Message} / 停止策略失败：{ex.Message}");
        }
    }

    public async Task<StrategyRuntimeStatusDto> GetStrategyRuntimeStatusAsync(
        int strategyId, 
        int userId, 
        CancellationToken cancellationToken = default)
    {
        // Verify ownership
        var strategy = await _context.Strategies
            .FirstOrDefaultAsync(s => s.Id == strategyId && s.UserId == userId, cancellationToken);

        if (strategy == null)
        {
            throw new InvalidOperationException("Strategy not found / 策略不存在");
        }

        var isRunning = _runningProcesses.TryGetValue(strategyId, out var runningInfo);
        
        if (!isRunning)
        {
            return new StrategyRuntimeStatusDto
            {
                StrategyId = strategyId,
                IsRunning = false,
                HealthStatus = "Stopped"
            };
        }

        var (process, execution) = runningInfo;
        var duration = (int)(DateTime.UtcNow - execution.StartedAt).TotalSeconds;

        // Get resource usage
        (double? cpu, long? memoryMB) = (null, null);
        if (_resourceUsage.TryGetValue(strategyId, out var usage))
        {
            cpu = usage.cpu;
            memoryMB = usage.memoryMB;
        }

        // Determine health status
        var healthStatus = "Unknown";
        try
        {
            if (!process.HasExited)
            {
                healthStatus = "Healthy";
            }
            else
            {
                healthStatus = "Crashed";
            }
        }
        catch
        {
            healthStatus = "Unhealthy";
        }

        return new StrategyRuntimeStatusDto
        {
            StrategyId = strategyId,
            IsRunning = true,
            CurrentExecutionId = execution.Id,
            ProcessId = process.Id,
            StartedAt = execution.StartedAt,
            DurationSeconds = duration,
            CpuUsage = cpu,
            MemoryUsageMB = memoryMB,
            OrdersExecuted = execution.OrdersExecuted,
            CurrentPositions = 0, // TODO: Get from broker
            CurrentPnL = null, // TODO: Calculate from positions
            HealthStatus = healthStatus
        };
    }

    public async Task<StrategyExecutionListResponse> GetStrategyExecutionsAsync(
        int strategyId, 
        int userId, 
        int page = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        // Verify ownership
        var strategy = await _context.Strategies
            .FirstOrDefaultAsync(s => s.Id == strategyId && s.UserId == userId, cancellationToken);

        if (strategy == null)
        {
            throw new InvalidOperationException("Strategy not found / 策略不存在");
        }

        var query = _context.StrategyExecutions
            .Include(e => e.Strategy)
            .Where(e => e.StrategyId == strategyId)
            .OrderByDescending(e => e.StartedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var executions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new StrategyExecutionListResponse
        {
            Executions = executions.Select(e => MapToExecutionDetail(e, strategy.Name)).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<StrategyExecutionDetailDto?> GetExecutionDetailsAsync(
        int executionId, 
        int userId, 
        CancellationToken cancellationToken = default)
    {
        var execution = await _context.StrategyExecutions
            .Include(e => e.Strategy)
            .FirstOrDefaultAsync(e => e.Id == executionId && e.Strategy.UserId == userId, cancellationToken);

        return execution != null ? MapToExecutionDetail(execution, execution.Strategy.Name) : null;
    }

    public async Task ScheduleStrategyAsync(
        int strategyId, 
        int userId, 
        ScheduleStrategyExecutionRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Verify ownership
        var strategy = await _context.Strategies
            .FirstOrDefaultAsync(s => s.Id == strategyId && s.UserId == userId, cancellationToken);

        if (strategy == null)
        {
            throw new InvalidOperationException("Strategy not found / 策略不存在");
        }

        // TODO: Implement scheduling logic using background service with System.Threading.Timer or Hangfire
        _logger.LogInformation("Scheduling strategy {StrategyId} with type {ScheduleType} at {StartTime}", 
            strategyId, request.ScheduleType, request.StartTime);
        
        // Store schedule in database (would require new StrategySchedule entity)
        // For now, just log the intent
        throw new NotImplementedException("Strategy scheduling will be implemented in task 12.9 / 策略调度功能将在任务 12.9 实现");
    }

    public async Task CancelScheduleAsync(
        int strategyId, 
        int userId, 
        CancellationToken cancellationToken = default)
    {
        // Verify ownership
        var strategy = await _context.Strategies
            .FirstOrDefaultAsync(s => s.Id == strategyId && s.UserId == userId, cancellationToken);

        if (strategy == null)
        {
            throw new InvalidOperationException("Strategy not found / 策略不存在");
        }

        // TODO: Remove schedule from background service
        _logger.LogInformation("Canceling schedule for strategy {StrategyId}", strategyId);
        
        throw new NotImplementedException("Schedule cancellation will be implemented in task 12.9 / 取消调度将在任务 12.9 实现");
    }

    public Task<bool> IsStrategyRunningAsync(int strategyId)
    {
        return Task.FromResult(_runningProcesses.ContainsKey(strategyId));
    }

    public Task<(double cpuUsage, long memoryMB)?> GetProcessResourceUsageAsync(int strategyId)
    {
        if (_resourceUsage.TryGetValue(strategyId, out var usage))
        {
            return Task.FromResult<(double, long)?>((usage.cpu, usage.memoryMB));
        }
        return Task.FromResult<(double, long)?>(null);
    }

    // ==================== Private Helper Methods ====================

    private async Task<Process> StartLeanProcessAsync(int strategyId, string configJson, int executionId)
    {
        // Create temporary config file
        var configDir = Path.Combine(Path.GetTempPath(), "WebUI", "Strategies", strategyId.ToString());
        Directory.CreateDirectory(configDir);
        
        var configPath = Path.Combine(configDir, $"config_{executionId}.json");
        await File.WriteAllTextAsync(configPath, configJson);

        // Find Lean executable (adjust path as needed)
        var leanPath = FindLeanExecutable();
        
        if (string.IsNullOrEmpty(leanPath) || !File.Exists(leanPath))
        {
            throw new FileNotFoundException($"Lean executable not found / Lean 可执行文件未找到: {leanPath}");
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = leanPath,
            Arguments = $"--config \"{configPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(leanPath)
        };

        var process = new Process
        {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true
        };

        // Set up log capture
        var logDir = Path.Combine(configDir, "logs");
        Directory.CreateDirectory(logDir);
        var logPath = Path.Combine(logDir, $"execution_{executionId}.log");

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                File.AppendAllText(logPath, e.Data + Environment.NewLine);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                File.AppendAllText(logPath, $"[ERROR] {e.Data}" + Environment.NewLine);
            }
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start Lean process / 启动 Lean 进程失败");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        _logger.LogInformation("Started Lean process {ProcessId} for strategy {StrategyId}", process.Id, strategyId);

        // Start log streaming to SignalR (if available)
        if (_logStreamingService != null)
        {
            await _logStreamingService.StartStreamingAsync(strategyId, logPath);
        }

        return process;
    }

    private string FindLeanExecutable()
    {
        // Try to find Lean executable in common locations
        var possiblePaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Launcher", "bin", "Debug", "QuantConnect.Lean.Launcher.exe"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Launcher", "bin", "Release", "QuantConnect.Lean.Launcher.exe"),
            Environment.GetEnvironmentVariable("LEAN_LAUNCHER_PATH") ?? string.Empty
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        // Default fallback
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QuantConnect.Lean.Launcher.exe");
    }

    private void OnProcessExited(int strategyId, int exitCode)
    {
        _logger.LogInformation("Strategy {StrategyId} process exited with code {ExitCode}", strategyId, exitCode);

        if (!_runningProcesses.TryRemove(strategyId, out var runningInfo))
        {
            return;
        }

        var (process, execution) = runningInfo;

        Task.Run(async () =>
        {
            try
            {
                // Update execution record
                execution.StoppedAt = DateTime.UtcNow;
                execution.Status = exitCode == 0 ? "Completed" : "Failed";
                
                if (exitCode != 0)
                {
                    execution.ErrorMessage = $"Process exited with code {exitCode} / 进程退出码 {exitCode}";
                }

                await _context.SaveChangesAsync();

                // Update strategy status
                var strategy = await _context.Strategies.FindAsync(strategyId);
                if (strategy != null)
                {
                    strategy.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                // Clean up
                _resourceUsage.TryRemove(strategyId, out _);

                // Auto-restart if enabled and crashed
                if (exitCode != 0 && _autoRestartFlags.TryGetValue(strategyId, out var autoRestart) && autoRestart)
                {
                    _logger.LogWarning("Strategy {StrategyId} crashed, will auto-restart in {Delay}s", 
                        strategyId, AutoRestartDelaySeconds);
                    
                    await Task.Delay(AutoRestartDelaySeconds * 1000);
                    
                    // Get latest strategy and user info
                    if (strategy != null)
                    {
                        await StartStrategyAsync(strategyId, strategy.UserId, new StartStrategyRequest 
                        { 
                            AutoRestart = true 
                        });
                    }
                }
                else
                {
                    _autoRestartFlags.TryRemove(strategyId, out _);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling process exit for strategy {StrategyId}", strategyId);
            }
        });
    }

    private async Task StartHealthMonitoringLoop()
    {
        while (true)
        {
            await Task.Delay(HealthCheckIntervalSeconds * 1000);

            foreach (var kvp in _runningProcesses.ToArray())
            {
                var strategyId = kvp.Key;
                var (process, execution) = kvp.Value;

                try
                {
                    if (process.HasExited)
                    {
                        _logger.LogWarning("Strategy {StrategyId} process has exited unexpectedly", strategyId);
                        continue;
                    }

                    // Refresh process info
                    process.Refresh();

                    // Calculate CPU usage (simplified - would need better implementation)
                    var cpuUsage = 0.0; // TODO: Implement proper CPU usage calculation
                    var memoryMB = process.WorkingSet64 / 1024 / 1024;

                    _resourceUsage[strategyId] = (cpuUsage, memoryMB, DateTime.UtcNow);

                    // TODO: Implement IPC health check (Named Pipe or TCP heartbeat)
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error monitoring strategy {StrategyId}", strategyId);
                }
            }
        }
    }

    private string MergeConfigWithOverrides(string baseConfigJson, string overridesJson)
    {
        try
        {
            var baseConfig = JsonDocument.Parse(baseConfigJson);
            var overrides = JsonDocument.Parse(overridesJson);

            // Simple merge logic - in production, use proper JSON merge
            // For now, just concatenate (this is simplified)
            return baseConfigJson; // TODO: Implement proper JSON merge
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to merge config overrides");
            return baseConfigJson;
        }
    }

    private decimal? ExtractInitialCapital(string configJson)
    {
        try
        {
            var config = JsonDocument.Parse(configJson);
            if (config.RootElement.TryGetProperty("initial-capital", out var capital))
            {
                return capital.GetDecimal();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract initial capital from config");
        }
        return null;
    }

    private StrategyExecutionDetailDto MapToExecutionDetail(StrategyExecution execution, string strategyName)
    {
        var duration = execution.StoppedAt.HasValue
            ? (int)(execution.StoppedAt.Value - execution.StartedAt).TotalSeconds
            : (int)(DateTime.UtcNow - execution.StartedAt).TotalSeconds;

        return new StrategyExecutionDetailDto
        {
            Id = execution.Id,
            StrategyId = execution.StrategyId,
            StrategyName = strategyName,
            Status = execution.Status,
            StartedAt = execution.StartedAt,
            StoppedAt = execution.StoppedAt,
            ProcessId = execution.ProcessId,
            LogFilePath = execution.LogFilePath,
            InitialCapital = execution.InitialCapital,
            FinalCapital = execution.FinalCapital,
            TotalReturn = execution.TotalReturn,
            DurationSeconds = duration,
            BrokerAccountId = execution.BrokerAccountId,
            ErrorMessage = execution.ErrorMessage,
            OrdersExecuted = execution.OrdersExecuted
        };
    }
}
