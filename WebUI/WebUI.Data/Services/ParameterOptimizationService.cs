using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models.Backtest;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Parameter optimization service / 参数优化服务
    /// </summary>
    public interface IParameterOptimizationService
    {
        /// <summary>
        /// Start parameter optimization / 开始参数优化
        /// </summary>
        Task<OptimizeParametersResponse> StartOptimizationAsync(OptimizeParametersRequest request, int userId);

        /// <summary>
        /// Get optimization status / 获取优化状态
        /// </summary>
        Task<OptimizeParametersResponse?> GetOptimizationAsync(int optimizationId, int userId);

        /// <summary>
        /// Get all optimizations for a user / 获取用户的所有优化
        /// </summary>
        Task<OptimizeParametersResponse[]> GetOptimizationsAsync(int userId, int? strategyId = null);

        /// <summary>
        /// Cancel optimization / 取消优化
        /// </summary>
        Task CancelOptimizationAsync(int optimizationId, int userId);
    }

    /// <summary>
    /// Parameter optimization service implementation / 参数优化服务实现
    /// </summary>
    public class ParameterOptimizationService : IParameterOptimizationService
    {
        private readonly WebUIDbContext _context;
        private readonly ILogger<ParameterOptimizationService> _logger;
        private readonly IBacktestService _backtestService;
        private readonly IBacktestExecutionService _backtestExecutionService;

        public ParameterOptimizationService(
            WebUIDbContext context,
            ILogger<ParameterOptimizationService> logger,
            IBacktestService backtestService,
            IBacktestExecutionService backtestExecutionService)
        {
            _context = context;
            _logger = logger;
            _backtestService = backtestService;
            _backtestExecutionService = backtestExecutionService;
        }

        public async Task<OptimizeParametersResponse> StartOptimizationAsync(OptimizeParametersRequest request, int userId)
        {
            _logger.LogInformation("Starting parameter optimization {Name} for strategy {StrategyId}", request.Name, request.StrategyId);

            // Validate strategy exists
            var strategy = await _context.Strategies
                .FirstOrDefaultAsync(s => s.Id == request.StrategyId && s.UserId == userId);

            if (strategy == null)
            {
                throw new InvalidOperationException("Strategy not found / 策略不存在");
            }

            // Generate parameter combinations
            var combinations = GenerateParameterCombinations(request.Parameters);
            
            if (combinations.Count == 0)
            {
                throw new InvalidOperationException("No parameter combinations generated / 未生成参数组合");
            }

            if (combinations.Count > 1000)
            {
                throw new InvalidOperationException($"Too many combinations ({combinations.Count}). Maximum is 1000 / 组合过多（{combinations.Count}），最大为 1000");
            }

            // Create optimization record
            var optimization = new Entities.ParameterOptimization
            {
                UserId = userId,
                StrategyId = request.StrategyId,
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                InitialCapital = request.InitialCapital,
                ParameterRangesJson = JsonSerializer.Serialize(request.Parameters),
                OptimizationMetric = request.OptimizationMetric,
                Status = "Pending",
                TotalCombinations = combinations.Count,
                CompletedCombinations = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.ParameterOptimizations.Add(optimization);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Optimization {OptimizationId} created with {Count} combinations", optimization.Id, combinations.Count);

            // Start optimization in background
            _ = RunOptimizationAsync(optimization.Id, request, combinations);

            return new OptimizeParametersResponse
            {
                OptimizationId = optimization.Id,
                Name = optimization.Name,
                Status = optimization.Status,
                TotalCombinations = optimization.TotalCombinations,
                CompletedCombinations = optimization.CompletedCombinations,
                CreatedAt = optimization.CreatedAt
            };
        }

        public async Task<OptimizeParametersResponse?> GetOptimizationAsync(int optimizationId, int userId)
        {
            var optimization = await _context.ParameterOptimizations
                .Include(o => o.Strategy)
                .Include(o => o.BestBacktest)
                .FirstOrDefaultAsync(o => o.Id == optimizationId && o.UserId == userId);

            if (optimization == null)
            {
                return null;
            }

            OptimizationResult[]? results = null;
            OptimizationResult? bestResult = null;

            if (!string.IsNullOrEmpty(optimization.ResultsJson))
            {
                results = JsonSerializer.Deserialize<OptimizationResult[]>(optimization.ResultsJson);
                bestResult = results?.FirstOrDefault(r => r.IsBest);
            }

            return new OptimizeParametersResponse
            {
                OptimizationId = optimization.Id,
                Name = optimization.Name,
                Status = optimization.Status,
                TotalCombinations = optimization.TotalCombinations,
                CompletedCombinations = optimization.CompletedCombinations,
                CreatedAt = optimization.CreatedAt,
                StartedAt = optimization.StartedAt,
                CompletedAt = optimization.CompletedAt,
                Results = results,
                BestResult = bestResult
            };
        }

        public async Task<OptimizeParametersResponse[]> GetOptimizationsAsync(int userId, int? strategyId = null)
        {
            var query = _context.ParameterOptimizations
                .Where(o => o.UserId == userId);

            if (strategyId.HasValue)
            {
                query = query.Where(o => o.StrategyId == strategyId.Value);
            }

            var optimizations = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToArrayAsync();

            return optimizations.Select(o => new OptimizeParametersResponse
            {
                OptimizationId = o.Id,
                Name = o.Name,
                Status = o.Status,
                TotalCombinations = o.TotalCombinations,
                CompletedCombinations = o.CompletedCombinations,
                CreatedAt = o.CreatedAt,
                StartedAt = o.StartedAt,
                CompletedAt = o.CompletedAt
            }).ToArray();
        }

        public async Task CancelOptimizationAsync(int optimizationId, int userId)
        {
            var optimization = await _context.ParameterOptimizations
                .FirstOrDefaultAsync(o => o.Id == optimizationId && o.UserId == userId);

            if (optimization == null)
            {
                throw new InvalidOperationException("Optimization not found / 优化不存在");
            }

            if (optimization.Status != "Running")
            {
                throw new InvalidOperationException("Optimization is not running / 优化未运行");
            }

            optimization.Status = "Failed";
            optimization.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Optimization {OptimizationId} cancelled", optimizationId);
        }

        private List<Dictionary<string, decimal>> GenerateParameterCombinations(ParameterRange[] parameters)
        {
            var combinations = new List<Dictionary<string, decimal>>();

            if (parameters.Length == 0)
            {
                return combinations;
            }

            // Generate all combinations using recursive approach
            GenerateCombinationsRecursive(parameters, 0, new Dictionary<string, decimal>(), combinations);

            return combinations;
        }

        private void GenerateCombinationsRecursive(
            ParameterRange[] parameters,
            int index,
            Dictionary<string, decimal> current,
            List<Dictionary<string, decimal>> combinations)
        {
            if (index >= parameters.Length)
            {
                combinations.Add(new Dictionary<string, decimal>(current));
                return;
            }

            var param = parameters[index];
            for (var value = param.Min; value <= param.Max; value += param.Step)
            {
                current[param.Name] = value;
                GenerateCombinationsRecursive(parameters, index + 1, current, combinations);
            }
        }

        private async Task RunOptimizationAsync(
            int optimizationId,
            OptimizeParametersRequest request,
            List<Dictionary<string, decimal>> combinations)
        {
            try
            {
                // Update status to Running
                var optimization = await _context.ParameterOptimizations.FindAsync(optimizationId);
                if (optimization == null) return;

                optimization.Status = "Running";
                optimization.StartedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Starting optimization {OptimizationId} with {Count} combinations", optimizationId, combinations.Count);

                var results = new List<OptimizationResult>();
                var semaphore = new SemaphoreSlim(request.MaxParallelBacktests);

                // Run backtests for each parameter combination
                var tasks = combinations.Select(async (combo, index) =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var result = await RunSingleBacktestAsync(request, combo, optimization.UserId);
                        results.Add(result);

                        // Update progress
                        optimization.CompletedCombinations = results.Count;
                        await _context.SaveChangesAsync();

                        _logger.LogDebug("Completed {Index}/{Total} for optimization {OptimizationId}", 
                            results.Count, combinations.Count, optimizationId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                // Find best result based on optimization metric
                var bestResult = FindBestResult(results, request.OptimizationMetric);
                if (bestResult != null)
                {
                    bestResult.IsBest = true;
                    optimization.BestBacktestId = bestResult.BacktestId;
                }

                // Save results
                optimization.ResultsJson = JsonSerializer.Serialize(results);
                optimization.Status = "Completed";
                optimization.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Optimization {OptimizationId} completed successfully. Best result: Backtest {BacktestId}",
                    optimizationId, bestResult?.BacktestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running optimization {OptimizationId}", optimizationId);
                
                var optimization = await _context.ParameterOptimizations.FindAsync(optimizationId);
                if (optimization != null)
                {
                    optimization.Status = "Failed";
                    optimization.CompletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task<OptimizationResult> RunSingleBacktestAsync(
            OptimizeParametersRequest request,
            Dictionary<string, decimal> parameters,
            int userId)
        {
            // Create backtest with specific parameters
            var backtestRequest = new CreateBacktestRequest
            {
                StrategyId = request.StrategyId,
                Name = $"{request.Name} - {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                InitialCapital = request.InitialCapital,
                DataResolution = "Daily",
                ParametersJson = JsonSerializer.Serialize(parameters)
            };

            var backtest = await _backtestService.CreateBacktestAsync(backtestRequest, userId);

            // Start backtest execution
            await _backtestExecutionService.StartBacktestAsync(backtest.Id, userId);

            // Wait for completion (with timeout)
            var timeout = TimeSpan.FromMinutes(30);
            var deadline = DateTime.UtcNow.Add(timeout);

            while (DateTime.UtcNow < deadline)
            {
                await Task.Delay(5000); // Check every 5 seconds

                var status = await _backtestService.GetBacktestByIdAsync(backtest.Id, userId);
                if (status == null) break;

                if (status.Status == "Completed")
                {
                    return new OptimizationResult
                    {
                        BacktestId = backtest.Id,
                        ParameterValues = JsonSerializer.Serialize(parameters),
                        MetricValue = GetMetricValue(status.Performance, request.OptimizationMetric),
                        TotalReturn = status.Performance?.TotalReturn,
                        SharpeRatio = status.Performance?.SharpeRatio,
                        MaxDrawdown = status.Performance?.MaxDrawdown,
                        TotalTrades = status.Performance?.TotalTrades
                    };
                }

                if (status.Status == "Failed")
                {
                    throw new InvalidOperationException($"Backtest {backtest.Id} failed");
                }
            }

            throw new TimeoutException($"Backtest {backtest.Id} timed out");
        }

        private decimal? GetMetricValue(BacktestPerformanceMetrics? metrics, string metricName)
        {
            if (metrics == null) return null;

            return metricName switch
            {
                "TotalReturn" => metrics.TotalReturn,
                "AnnualReturn" => metrics.AnnualReturn,
                "SharpeRatio" => metrics.SharpeRatio,
                "MaxDrawdown" => metrics.MaxDrawdown,
                "WinRate" => metrics.WinRate,
                "ProfitFactor" => metrics.ProfitFactor,
                _ => metrics.SharpeRatio
            };
        }

        private OptimizationResult? FindBestResult(List<OptimizationResult> results, string metricName)
        {
            if (results.Count == 0) return null;

            // For MaxDrawdown, lower is better
            if (metricName == "MaxDrawdown")
            {
                return results
                    .Where(r => r.MetricValue.HasValue)
                    .OrderBy(r => r.MetricValue)
                    .FirstOrDefault();
            }

            // For other metrics, higher is better
            return results
                .Where(r => r.MetricValue.HasValue)
                .OrderByDescending(r => r.MetricValue)
                .FirstOrDefault();
        }
    }
}
