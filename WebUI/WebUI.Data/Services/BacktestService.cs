using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models.Backtest;
using WebUI.Data.Entities;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Backtest service implementation / 回测服务实现
    /// </summary>
    public class BacktestService : IBacktestService
    {
        private readonly WebUIDbContext _context;
        private readonly ILogger<BacktestService> _logger;

        public BacktestService(WebUIDbContext context, ILogger<BacktestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BacktestDetailDto> CreateBacktestAsync(CreateBacktestRequest request, int userId)
        {
            _logger.LogInformation("Creating backtest {Name} for user {UserId}", request.Name, userId);

            // Validate date range
            if (request.EndDate <= request.StartDate)
            {
                throw new ArgumentException("End date must be after start date / 结束日期必须晚于开始日期");
            }

            // Validate strategy exists
            var strategy = await _context.Strategies
                .FirstOrDefaultAsync(s => s.Id == request.StrategyId && s.UserId == userId);

            if (strategy == null)
            {
                throw new ArgumentException("Strategy not found / 策略不存在");
            }

            var backtest = new Entities.Backtest
            {
                UserId = userId,
                StrategyId = request.StrategyId,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                InitialCapital = request.InitialCapital,
                BenchmarkSymbol = request.BenchmarkSymbol,
                DataResolution = request.DataResolution,
                ParametersJson = request.ParametersJson,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Backtests.Add(backtest);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Backtest {BacktestId} created successfully", backtest.Id);

            return await GetBacktestByIdAsync(backtest.Id, userId) 
                ?? throw new InvalidOperationException("Failed to retrieve created backtest");
        }

        public async Task<BacktestListResponse> GetBacktestsAsync(int userId, int? strategyId = null, string? status = null)
        {
            var query = _context.Backtests
                .Include(b => b.Strategy)
                .Where(b => b.UserId == userId);

            if (strategyId.HasValue)
            {
                query = query.Where(b => b.StrategyId == strategyId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            var backtests = await query
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BacktestDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    StrategyId = b.StrategyId,
                    StrategyName = b.Strategy.Name,
                    Name = b.Name,
                    Description = b.Description,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    InitialCapital = b.InitialCapital,
                    BenchmarkSymbol = b.BenchmarkSymbol,
                    DataResolution = b.DataResolution,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    StartedAt = b.StartedAt,
                    CompletedAt = b.CompletedAt,
                    Progress = b.Progress,
                    ErrorMessage = b.ErrorMessage,
                    TotalReturn = b.TotalReturn,
                    AnnualReturn = b.AnnualReturn,
                    SharpeRatio = b.SharpeRatio,
                    MaxDrawdown = b.MaxDrawdown,
                    WinRate = b.WinRate,
                    TotalTrades = b.TotalTrades
                })
                .ToArrayAsync();

            return new BacktestListResponse
            {
                Total = backtests.Length,
                Backtests = backtests
            };
        }

        public async Task<BacktestDetailDto?> GetBacktestByIdAsync(int id, int userId)
        {
            var backtest = await _context.Backtests
                .Include(b => b.Strategy)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (backtest == null)
            {
                return null;
            }

            return new BacktestDetailDto
            {
                Id = backtest.Id,
                UserId = backtest.UserId,
                StrategyId = backtest.StrategyId,
                StrategyName = backtest.Strategy.Name,
                Name = backtest.Name,
                Description = backtest.Description,
                StartDate = backtest.StartDate,
                EndDate = backtest.EndDate,
                InitialCapital = backtest.InitialCapital,
                BenchmarkSymbol = backtest.BenchmarkSymbol,
                DataResolution = backtest.DataResolution,
                Status = backtest.Status,
                CreatedAt = backtest.CreatedAt,
                StartedAt = backtest.StartedAt,
                CompletedAt = backtest.CompletedAt,
                Progress = backtest.Progress,
                ErrorMessage = backtest.ErrorMessage,
                Parameters = string.IsNullOrEmpty(backtest.ParametersJson) 
                    ? null 
                    : JsonSerializer.Deserialize<JsonElement>(backtest.ParametersJson),
                Performance = new BacktestPerformanceMetrics
                {
                    TotalReturn = backtest.TotalReturn,
                    AnnualReturn = backtest.AnnualReturn,
                    SharpeRatio = backtest.SharpeRatio,
                    MaxDrawdown = backtest.MaxDrawdown,
                    WinRate = backtest.WinRate,
                    TotalTrades = backtest.TotalTrades,
                    WinningTrades = backtest.WinningTrades,
                    LosingTrades = backtest.LosingTrades,
                    AverageWin = backtest.AverageWin,
                    AverageLoss = backtest.AverageLoss,
                    ProfitFactor = backtest.ProfitFactor,
                    Alpha = backtest.Alpha,
                    Beta = backtest.Beta
                },
                Charts = new BacktestChartsData
                {
                    EquityCurve = string.IsNullOrEmpty(backtest.EquityCurveJson)
                        ? null
                        : JsonSerializer.Deserialize<EquityPoint[]>(backtest.EquityCurveJson),
                    DrawdownCurve = string.IsNullOrEmpty(backtest.DrawdownCurveJson)
                        ? null
                        : JsonSerializer.Deserialize<DrawdownPoint[]>(backtest.DrawdownCurveJson)
                },
                Trades = string.IsNullOrEmpty(backtest.TradesJson)
                    ? null
                    : JsonSerializer.Deserialize<BacktestTrade[]>(backtest.TradesJson)
            };
        }

        public async Task<bool> DeleteBacktestAsync(int id, int userId)
        {
            var backtest = await _context.Backtests
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (backtest == null)
            {
                return false;
            }

            _context.Backtests.Remove(backtest);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Backtest {BacktestId} deleted successfully", id);
            return true;
        }

        public async Task UpdateBacktestStatusAsync(int id, string status, int? progress = null, string? errorMessage = null)
        {
            var backtest = await _context.Backtests.FindAsync(id);
            if (backtest == null)
            {
                throw new ArgumentException($"Backtest {id} not found");
            }

            backtest.Status = status;
            backtest.Progress = progress;
            backtest.ErrorMessage = errorMessage;

            if (status == "Running" && backtest.StartedAt == null)
            {
                backtest.StartedAt = DateTime.UtcNow;
            }

            if (status == "Completed" || status == "Failed")
            {
                backtest.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Backtest {BacktestId} status updated to {Status}", id, status);
        }

        public async Task UpdateBacktestResultsAsync(int id, BacktestPerformanceMetrics metrics, BacktestChartsData charts, BacktestTrade[] trades)
        {
            var backtest = await _context.Backtests.FindAsync(id);
            if (backtest == null)
            {
                throw new ArgumentException($"Backtest {id} not found");
            }

            // Update performance metrics
            backtest.TotalReturn = metrics.TotalReturn;
            backtest.AnnualReturn = metrics.AnnualReturn;
            backtest.SharpeRatio = metrics.SharpeRatio;
            backtest.MaxDrawdown = metrics.MaxDrawdown;
            backtest.WinRate = metrics.WinRate;
            backtest.TotalTrades = metrics.TotalTrades;
            backtest.WinningTrades = metrics.WinningTrades;
            backtest.LosingTrades = metrics.LosingTrades;
            backtest.AverageWin = metrics.AverageWin;
            backtest.AverageLoss = metrics.AverageLoss;
            backtest.ProfitFactor = metrics.ProfitFactor;
            backtest.Alpha = metrics.Alpha;
            backtest.Beta = metrics.Beta;

            // Update charts
            if (charts.EquityCurve != null)
            {
                backtest.EquityCurveJson = JsonSerializer.Serialize(charts.EquityCurve);
            }

            if (charts.DrawdownCurve != null)
            {
                backtest.DrawdownCurveJson = JsonSerializer.Serialize(charts.DrawdownCurve);
            }

            // Update trades
            if (trades != null)
            {
                backtest.TradesJson = JsonSerializer.Serialize(trades);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Backtest {BacktestId} results updated successfully", id);
        }

        public async Task<CompareBacktestsResponse> CompareBacktestsAsync(int[] backtestIds, int userId)
        {
            if (backtestIds == null || backtestIds.Length < 2 || backtestIds.Length > 4)
            {
                throw new ArgumentException("Must provide 2-4 backtest IDs for comparison / 必须提供 2-4 个回测 ID 进行对比");
            }

            _logger.LogInformation("Comparing {Count} backtests for user {UserId}", backtestIds.Length, userId);

            var backtests = await _context.Backtests
                .Include(b => b.Strategy)
                .Where(b => backtestIds.Contains(b.Id) && b.UserId == userId && b.Status == "Completed")
                .ToArrayAsync();

            if (backtests.Length != backtestIds.Length)
            {
                throw new ArgumentException("One or more backtests not found or not completed / 一个或多个回测不存在或未完成");
            }

            // Determine best backtest by Sharpe Ratio
            var bestBacktest = backtests
                .Where(b => b.SharpeRatio.HasValue)
                .OrderByDescending(b => b.SharpeRatio)
                .FirstOrDefault();

            var comparisonItems = backtests.Select(b => new BacktestComparisonItem
            {
                Id = b.Id,
                Name = b.Name,
                StrategyName = b.Strategy.Name,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                InitialCapital = b.InitialCapital,
                BenchmarkSymbol = b.BenchmarkSymbol,
                IsBest = bestBacktest != null && b.Id == bestBacktest.Id,
                Performance = new BacktestPerformanceMetrics
                {
                    TotalReturn = b.TotalReturn,
                    AnnualReturn = b.AnnualReturn,
                    SharpeRatio = b.SharpeRatio,
                    MaxDrawdown = b.MaxDrawdown,
                    WinRate = b.WinRate,
                    TotalTrades = b.TotalTrades,
                    WinningTrades = b.WinningTrades,
                    LosingTrades = b.LosingTrades,
                    AverageWin = b.AverageWin,
                    AverageLoss = b.AverageLoss,
                    ProfitFactor = b.ProfitFactor,
                    Alpha = b.Alpha,
                    Beta = b.Beta
                },
                EquityCurve = string.IsNullOrEmpty(b.EquityCurveJson)
                    ? null
                    : JsonSerializer.Deserialize<EquityPoint[]>(b.EquityCurveJson)
            }).ToArray();

            return new CompareBacktestsResponse
            {
                Backtests = comparisonItems,
                BestBacktestId = bestBacktest?.Id,
                BestBy = "SharpeRatio"
            };
        }
    }
}
