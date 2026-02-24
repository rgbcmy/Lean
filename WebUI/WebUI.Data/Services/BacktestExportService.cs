using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models.Backtest;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Backtest export service / 回测导出服务
    /// </summary>
    public interface IBacktestExportService
    {
        /// <summary>
        /// Export backtest as JSON / 导出回测为 JSON
        /// </summary>
        Task<byte[]> ExportAsJsonAsync(int backtestId, int userId);

        /// <summary>
        /// Export backtest trades as CSV / 导出回测交易记录为 CSV
        /// </summary>
        Task<byte[]> ExportTradesAsCsvAsync(int backtestId, int userId);

        /// <summary>
        /// Export backtest report as HTML / 导出回测报告为 HTML
        /// </summary>
        Task<byte[]> ExportAsHtmlAsync(int backtestId, int userId);
    }

    /// <summary>
    /// Backtest export service implementation / 回测导出服务实现
    /// </summary>
    public class BacktestExportService : IBacktestExportService
    {
        private readonly ILogger<BacktestExportService> _logger;
        private readonly IBacktestService _backtestService;

        public BacktestExportService(
            ILogger<BacktestExportService> logger,
            IBacktestService backtestService)
        {
            _logger = logger;
            _backtestService = backtestService;
        }

        public async Task<byte[]> ExportAsJsonAsync(int backtestId, int userId)
        {
            _logger.LogInformation("Exporting backtest {BacktestId} as JSON", backtestId);

            var backtest = await _backtestService.GetBacktestByIdAsync(backtestId, userId);
            
            if (backtest == null)
            {
                throw new InvalidOperationException("Backtest not found / 回测不存在");
            }

            var json = JsonSerializer.Serialize(backtest, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> ExportTradesAsCsvAsync(int backtestId, int userId)
        {
            _logger.LogInformation("Exporting backtest {BacktestId} trades as CSV", backtestId);

            var backtest = await _backtestService.GetBacktestByIdAsync(backtestId, userId);
            
            if (backtest == null)
            {
                throw new InvalidOperationException("Backtest not found / 回测不存在");
            }

            if (backtest.Trades == null || backtest.Trades.Length == 0)
            {
                throw new InvalidOperationException("No trades to export / 无交易记录可导出");
            }

            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("EntryTime,ExitTime,Symbol,Direction,EntryPrice,ExitPrice,Quantity,ProfitLoss,ProfitLossPercent,MAE,MFE");

            // Data rows
            foreach (var trade in backtest.Trades)
            {
                csv.AppendLine($"{trade.EntryTime:yyyy-MM-dd HH:mm:ss}," +
                              $"{trade.ExitTime:yyyy-MM-dd HH:mm:ss}," +
                              $"{trade.Symbol}," +
                              $"{trade.Direction}," +
                              $"{trade.EntryPrice:F2}," +
                              $"{trade.ExitPrice:F2}," +
                              $"{trade.Quantity}," +
                              $"{trade.ProfitLoss:F2}," +
                              $"{trade.ProfitLossPercent:F2}," +
                              $"{trade.Mae:F2}," +
                              $"{trade.Mfe:F2}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportAsHtmlAsync(int backtestId, int userId)
        {
            _logger.LogInformation("Exporting backtest {BacktestId} as HTML report", backtestId);

            var backtest = await _backtestService.GetBacktestByIdAsync(backtestId, userId);
            
            if (backtest == null)
            {
                throw new InvalidOperationException("Backtest not found / 回测不存在");
            }

            var html = GenerateHtmlReport(backtest);
            return Encoding.UTF8.GetBytes(html);
        }

        private string GenerateHtmlReport(BacktestDetailDto backtest)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine($"    <title>Backtest Report - {backtest.Name}</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 40px; }");
            sb.AppendLine("        h1 { color: #333; }");
            sb.AppendLine("        h2 { color: #666; margin-top: 30px; }");
            sb.AppendLine("        table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
            sb.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            sb.AppendLine("        th { background-color: #4CAF50; color: white; }");
            sb.AppendLine("        .metric { display: inline-block; margin: 10px 20px 10px 0; }");
            sb.AppendLine("        .metric-label { font-weight: bold; color: #666; }");
            sb.AppendLine("        .metric-value { color: #333; font-size: 18px; }");
            sb.AppendLine("        .positive { color: green; }");
            sb.AppendLine("        .negative { color: red; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            
            // Header
            sb.AppendLine($"    <h1>Backtest Report: {backtest.Name}</h1>");
            sb.AppendLine($"    <p><strong>Strategy:</strong> {backtest.StrategyName}</p>");
            sb.AppendLine($"    <p><strong>Period:</strong> {backtest.StartDate:yyyy-MM-dd} to {backtest.EndDate:yyyy-MM-dd}</p>");
            sb.AppendLine($"    <p><strong>Initial Capital:</strong> ${backtest.InitialCapital:N2}</p>");
            
            if (!string.IsNullOrEmpty(backtest.BenchmarkSymbol))
            {
                sb.AppendLine($"    <p><strong>Benchmark:</strong> {backtest.BenchmarkSymbol}</p>");
            }
            
            // Performance Metrics
            if (backtest.Performance != null)
            {
                sb.AppendLine("    <h2>Performance Metrics</h2>");
                sb.AppendLine("    <div>");
                
                AddMetric(sb, "Total Return", backtest.Performance.TotalReturn, "%", true);
                AddMetric(sb, "Annual Return", backtest.Performance.AnnualReturn, "%", true);
                AddMetric(sb, "Sharpe Ratio", backtest.Performance.SharpeRatio, "", false);
                AddMetric(sb, "Max Drawdown", backtest.Performance.MaxDrawdown, "%", true, isNegative: true);
                AddMetric(sb, "Win Rate", backtest.Performance.WinRate, "%", false);
                AddMetric(sb, "Total Trades", backtest.Performance.TotalTrades, "", false);
                AddMetric(sb, "Winning Trades", backtest.Performance.WinningTrades, "", false);
                AddMetric(sb, "Losing Trades", backtest.Performance.LosingTrades, "", false);
                AddMetric(sb, "Profit Factor", backtest.Performance.ProfitFactor, "", false);
                
                if (backtest.Performance.Alpha.HasValue)
                {
                    AddMetric(sb, "Alpha", backtest.Performance.Alpha, "", false);
                }
                
                if (backtest.Performance.Beta.HasValue)
                {
                    AddMetric(sb, "Beta", backtest.Performance.Beta, "", false);
                }
                
                sb.AppendLine("    </div>");
            }
            
            // Trades Table
            if (backtest.Trades != null && backtest.Trades.Length > 0)
            {
                sb.AppendLine("    <h2>Trade History</h2>");
                sb.AppendLine("    <table>");
                sb.AppendLine("        <tr>");
                sb.AppendLine("            <th>Entry Time</th>");
                sb.AppendLine("            <th>Exit Time</th>");
                sb.AppendLine("            <th>Symbol</th>");
                sb.AppendLine("            <th>Direction</th>");
                sb.AppendLine("            <th>Entry Price</th>");
                sb.AppendLine("            <th>Exit Price</th>");
                sb.AppendLine("            <th>Quantity</th>");
                sb.AppendLine("            <th>P&L</th>");
                sb.AppendLine("            <th>P&L %</th>");
                sb.AppendLine("        </tr>");
                
                foreach (var trade in backtest.Trades.Take(100)) // Limit to first 100 trades
                {
                    var plClass = trade.ProfitLoss >= 0 ? "positive" : "negative";
                    sb.AppendLine("        <tr>");
                    sb.AppendLine($"            <td>{trade.EntryTime:yyyy-MM-dd HH:mm}</td>");
                    sb.AppendLine($"            <td>{trade.ExitTime:yyyy-MM-dd HH:mm}</td>");
                    sb.AppendLine($"            <td>{trade.Symbol}</td>");
                    sb.AppendLine($"            <td>{trade.Direction}</td>");
                    sb.AppendLine($"            <td>${trade.EntryPrice:F2}</td>");
                    sb.AppendLine($"            <td>${trade.ExitPrice:F2}</td>");
                    sb.AppendLine($"            <td>{trade.Quantity}</td>");
                    sb.AppendLine($"            <td class=\"{plClass}\">${trade.ProfitLoss:F2}</td>");
                    sb.AppendLine($"            <td class=\"{plClass}\">{trade.ProfitLossPercent:F2}%</td>");
                    sb.AppendLine("        </tr>");
                }
                
                if (backtest.Trades.Length > 100)
                {
                    sb.AppendLine($"        <tr><td colspan=\"9\">... and {backtest.Trades.Length - 100} more trades</td></tr>");
                }
                
                sb.AppendLine("    </table>");
            }
            
            // Footer
            sb.AppendLine($"    <p style=\"margin-top: 40px; color: #999;\">Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }

        private void AddMetric(StringBuilder sb, string label, decimal? value, string suffix, bool isPercentage, bool isNegative = false)
        {
            if (!value.HasValue) return;
            
            var displayValue = isPercentage ? (value.Value * 100) : value.Value;
            var cssClass = "";
            
            if (!isNegative && displayValue > 0)
                cssClass = "positive";
            else if (isNegative && displayValue < 0)
                cssClass = "positive";
            else if (displayValue < 0)
                cssClass = "negative";
            
            sb.AppendLine("    <div class=\"metric\">");
            sb.AppendLine($"        <div class=\"metric-label\">{label}:</div>");
            sb.AppendLine($"        <div class=\"metric-value {cssClass}\">{displayValue:F2}{suffix}</div>");
            sb.AppendLine("    </div>");
        }

        private void AddMetric(StringBuilder sb, string label, int? value, string suffix, bool isPercentage)
        {
            if (!value.HasValue) return;
            
            sb.AppendLine("    <div class=\"metric\">");
            sb.AppendLine($"        <div class=\"metric-label\">{label}:</div>");
            sb.AppendLine($"        <div class=\"metric-value\">{value.Value}{suffix}</div>");
            sb.AppendLine("    </div>");
        }
    }
}
