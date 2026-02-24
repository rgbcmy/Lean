using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models.Backtest;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Backtest execution service implementation / 回测执行服务实现
    /// Manages Lean backtest process lifecycle
    /// </summary>
    public class BacktestExecutionService : IBacktestExecutionService, IDisposable
    {
        private readonly WebUIDbContext _context;
        private readonly ILogger<BacktestExecutionService> _logger;
        private readonly IBacktestService _backtestService;
        private readonly IConfiguration _configuration;
        
        // Track running backtest processes: backtestId -> (process, cancellationTokenSource)
        private readonly ConcurrentDictionary<int, (Process process, CancellationTokenSource cts)> _runningBacktests = new();
        
        private readonly string _leanExecutablePath;
        private readonly string _leanDataPath;
        private readonly string _backtestOutputPath;
        private bool _disposed;

        public BacktestExecutionService(
            WebUIDbContext context,
            ILogger<BacktestExecutionService> logger,
            IBacktestService backtestService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _backtestService = backtestService;
            _configuration = configuration;
            
            // Get Lean paths from configuration
            _leanExecutablePath = configuration["WebUI:LeanExecutablePath"] ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "Launcher", "bin", "Debug", "QuantConnect.Lean.Launcher.exe");
            _leanDataPath = configuration["WebUI:LeanDataPath"] ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "Data");
            _backtestOutputPath = configuration["WebUI:BacktestOutputPath"] ?? Path.Combine(AppContext.BaseDirectory, "BacktestResults");
            
            // Create output directory if it doesn't exist
            Directory.CreateDirectory(_backtestOutputPath);
            
            _logger.LogInformation("BacktestExecutionService initialized. Lean path: {LeanPath}, Data path: {DataPath}, Output path: {OutputPath}",
                _leanExecutablePath, _leanDataPath, _backtestOutputPath);
        }

        public async Task StartBacktestAsync(int backtestId, int userId)
        {
            _logger.LogInformation("Starting backtest {BacktestId} for user {UserId}", backtestId, userId);

            // Check if already running
            if (_runningBacktests.ContainsKey(backtestId))
            {
                throw new InvalidOperationException("Backtest is already running / 回测正在运行中");
            }

            // Get backtest configuration
            var backtest = await _context.Backtests
                .Include(b => b.Strategy)
                .FirstOrDefaultAsync(b => b.Id == backtestId && b.UserId == userId);

            if (backtest == null)
            {
                throw new InvalidOperationException("Backtest not found / 回测不存在");
            }

            if (backtest.Strategy == null)
            {
                throw new InvalidOperationException("Strategy not found / 策略不存在");
            }

            // Update status to Running
            await _backtestService.UpdateBacktestStatusAsync(backtestId, "Running", 0);

            try
            {
                // Generate Lean configuration file for backtest
                var configPath = await GenerateLeanBacktestConfigAsync(backtest);
                
                // Create result output path
                var resultPath = Path.Combine(_backtestOutputPath, $"backtest_{backtestId}");
                Directory.CreateDirectory(resultPath);

                // Start Lean process
                var process = StartLeanProcess(configPath, resultPath);
                var cts = new CancellationTokenSource();
                
                _runningBacktests[backtestId] = (process, cts);
                
                // Monitor process in background
                _ = MonitorBacktestProcessAsync(backtestId, process, resultPath, cts.Token);
                
                _logger.LogInformation("Backtest {BacktestId} started successfully. PID: {ProcessId}", backtestId, process.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start backtest {BacktestId}", backtestId);
                await _backtestService.UpdateBacktestStatusAsync(backtestId, "Failed", null, ex.Message);
                throw;
            }
        }

        public async Task StopBacktestAsync(int backtestId, int userId)
        {
            _logger.LogInformation("Stopping backtest {BacktestId} for user {UserId}", backtestId, userId);

            if (!_runningBacktests.TryRemove(backtestId, out var backtestInfo))
            {
                throw new InvalidOperationException("Backtest is not running / 回测未运行");
            }

            var (process, cts) = backtestInfo;
            
            try
            {
                // Cancel monitoring
                cts.Cancel();
                
                // Try graceful shutdown first
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    
                    // Wait for graceful shutdown (max 10 seconds)
                    if (!process.WaitForExit(10000))
                    {
                        // Force kill if not exited
                        _logger.LogWarning("Backtest {BacktestId} did not stop gracefully, forcing termination", backtestId);
                        process.Kill();
                    }
                }

                // Update status
                await _backtestService.UpdateBacktestStatusAsync(backtestId, "Failed", null, "Stopped by user / 用户手动停止");
                
                _logger.LogInformation("Backtest {BacktestId} stopped successfully", backtestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping backtest {BacktestId}", backtestId);
                throw;
            }
            finally
            {
                process.Dispose();
                cts.Dispose();
            }
        }

        public Task<BacktestStatusDto?> GetBacktestStatusAsync(int backtestId)
        {
            if (_runningBacktests.TryGetValue(backtestId, out var backtestInfo))
            {
                var (process, _) = backtestInfo;
                return Task.FromResult<BacktestStatusDto?>(new BacktestStatusDto
                {
                    BacktestId = backtestId,
                    Status = process.HasExited ? "Completed" : "Running",
                    Progress = null // TODO: Parse from logs
                });
            }

            return Task.FromResult<BacktestStatusDto?>(null);
        }

        public bool IsBacktestRunning(int backtestId)
        {
            return _runningBacktests.ContainsKey(backtestId);
        }

        private async Task<string> GenerateLeanBacktestConfigAsync(Entities.Backtest backtest)
        {
            // Generate Lean configuration JSON for backtest
            var config = new
            {
                algorithm_type_name = backtest.Strategy.StrategyType,
                algorithm_language = "CSharp",
                algorithm_location = backtest.Strategy.CodeFilePath ?? "QuantConnect.Algorithm.CSharp.dll",
                data_folder = _leanDataPath,
                
                // Backtest parameters
                start_date = backtest.StartDate.ToString("yyyyMMdd"),
                end_date = backtest.EndDate.ToString("yyyyMMdd"),
                cash = new { USD = backtest.InitialCapital },
                
                // Data resolution
                resolution = backtest.DataResolution,
                
                // Benchmark
                benchmark_symbol = backtest.BenchmarkSymbol ?? "SPY",
                
                // Output
                results_destination_folder = Path.Combine(_backtestOutputPath, $"backtest_{backtest.Id}"),
                
                // Additional parameters from strategy
                parameters = string.IsNullOrEmpty(backtest.ParametersJson)
                    ? new { }
                    : JsonSerializer.Deserialize<object>(backtest.ParametersJson)
            };

            var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            var configPath = Path.Combine(_backtestOutputPath, $"backtest_{backtest.Id}_config.json");
            
            await File.WriteAllTextAsync(configPath, configJson);
            _logger.LogDebug("Generated Lean config for backtest {BacktestId} at {ConfigPath}", backtest.Id, configPath);
            
            return configPath;
        }

        private Process StartLeanProcess(string configPath, string resultPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _leanExecutablePath,
                Arguments = $"--config \"{configPath}\" --results-destination-folder \"{resultPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_leanExecutablePath) ?? AppContext.BaseDirectory
            };

            var process = new Process { StartInfo = startInfo };
            
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogDebug("Backtest output: {Output}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogWarning("Backtest error: {Error}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        private async Task MonitorBacktestProcessAsync(int backtestId, Process process, string resultPath, CancellationToken cancellationToken)
        {
            try
            {
                while (!process.HasExited && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(2000, cancellationToken);
                    
                    // TODO: Parse log file to update progress
                    // For now, just keep the status as "Running"
                }

                // Check if cancelled
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Backtest {BacktestId} monitoring cancelled", backtestId);
                    return;
                }

                // Process exited
                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Backtest {BacktestId} completed successfully", backtestId);
                    
                    // Parse results and update database
                    await ParseAndSaveBacktestResultsAsync(backtestId, resultPath);
                    
                    await _backtestService.UpdateBacktestStatusAsync(backtestId, "Completed", 100);
                }
                else
                {
                    _logger.LogError("Backtest {BacktestId} failed with exit code {ExitCode}", backtestId, process.ExitCode);
                    await _backtestService.UpdateBacktestStatusAsync(backtestId, "Failed", null, $"Process exited with code {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring backtest {BacktestId}", backtestId);
                await _backtestService.UpdateBacktestStatusAsync(backtestId, "Failed", null, ex.Message);
            }
            finally
            {
                // Remove from running backtests
                _runningBacktests.TryRemove(backtestId, out _);
                process.Dispose();
            }
        }

        private async Task ParseAndSaveBacktestResultsAsync(int backtestId, string resultPath)
        {
            try
            {
                // Look for Lean result JSON file
                var resultFiles = Directory.GetFiles(resultPath, "*.json");
                
                if (resultFiles.Length == 0)
                {
                    _logger.LogWarning("No result files found for backtest {BacktestId}", backtestId);
                    return;
                }

                // Read the first result file (Lean typically outputs one main result file)
                var resultJson = await File.ReadAllTextAsync(resultFiles[0]);
                var result = JsonSerializer.Deserialize<LeanBacktestResult>(resultJson);

                if (result == null)
                {
                    _logger.LogWarning("Failed to parse backtest result for {BacktestId}", backtestId);
                    return;
                }

                // Extract metrics
                var metrics = new BacktestPerformanceMetrics
                {
                    TotalReturn = result.Statistics?.TotalReturn,
                    AnnualReturn = result.Statistics?.AnnualReturn,
                    SharpeRatio = result.Statistics?.SharpeRatio,
                    MaxDrawdown = result.Statistics?.MaxDrawdown,
                    WinRate = result.Statistics?.WinRate,
                    TotalTrades = result.Statistics?.TotalTrades,
                    WinningTrades = result.Statistics?.WinningTrades,
                    LosingTrades = result.Statistics?.LosingTrades,
                    AverageWin = result.Statistics?.AverageWin,
                    AverageLoss = result.Statistics?.AverageLoss,
                    ProfitFactor = result.Statistics?.ProfitFactor,
                    Alpha = result.Statistics?.Alpha,
                    Beta = result.Statistics?.Beta
                };

                // Extract equity curve
                var charts = new BacktestChartsData
                {
                    EquityCurve = result.Charts?.Equity?.Select(e => new EquityPoint
                    {
                        Time = e.Time,
                        Value = e.Value
                    }).ToArray(),
                    
                    DrawdownCurve = result.Charts?.Drawdown?.Select(d => new DrawdownPoint
                    {
                        Time = d.Time,
                        Value = d.Value
                    }).ToArray()
                };

                // Extract trades
                var trades = result.Orders?.Select(o => new BacktestTrade
                {
                    EntryTime = o.EntryTime,
                    ExitTime = o.ExitTime,
                    Symbol = o.Symbol,
                    Direction = o.Direction,
                    EntryPrice = o.EntryPrice,
                    ExitPrice = o.ExitPrice,
                    Quantity = o.Quantity,
                    ProfitLoss = o.ProfitLoss,
                    ProfitLossPercent = o.ProfitLossPercent,
                    Mae = o.Mae,
                    Mfe = o.Mfe
                }).ToArray() ?? Array.Empty<BacktestTrade>();

                // Update backtest with results
                await _backtestService.UpdateBacktestResultsAsync(backtestId, metrics, charts, trades);
                
                _logger.LogInformation("Backtest {BacktestId} results saved successfully", backtestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing backtest results for {BacktestId}", backtestId);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            // Stop all running backtests
            foreach (var kvp in _runningBacktests.ToArray())
            {
                try
                {
                    var (process, cts) = kvp.Value;
                    cts.Cancel();
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                    process.Dispose();
                    cts.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing backtest {BacktestId}", kvp.Key);
                }
            }
            
            _runningBacktests.Clear();
            _disposed = true;
        }

        // Lean result models for deserialization
        private class LeanBacktestResult
        {
            public StatisticsResult? Statistics { get; set; }
            public ChartsResult? Charts { get; set; }
            public TradeResult[]? Orders { get; set; }
        }

        private class StatisticsResult
        {
            public decimal? TotalReturn { get; set; }
            public decimal? AnnualReturn { get; set; }
            public decimal? SharpeRatio { get; set; }
            public decimal? MaxDrawdown { get; set; }
            public decimal? WinRate { get; set; }
            public int? TotalTrades { get; set; }
            public int? WinningTrades { get; set; }
            public int? LosingTrades { get; set; }
            public decimal? AverageWin { get; set; }
            public decimal? AverageLoss { get; set; }
            public decimal? ProfitFactor { get; set; }
            public decimal? Alpha { get; set; }
            public decimal? Beta { get; set; }
        }

        private class ChartsResult
        {
            public ChartPoint[]? Equity { get; set; }
            public ChartPoint[]? Drawdown { get; set; }
        }

        private class ChartPoint
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
        }

        private class TradeResult
        {
            public DateTime EntryTime { get; set; }
            public DateTime ExitTime { get; set; }
            public string Symbol { get; set; } = string.Empty;
            public string Direction { get; set; } = string.Empty;
            public decimal EntryPrice { get; set; }
            public decimal ExitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal ProfitLoss { get; set; }
            public decimal ProfitLossPercent { get; set; }
            public decimal Mae { get; set; }
            public decimal Mfe { get; set; }
        }
    }
}
