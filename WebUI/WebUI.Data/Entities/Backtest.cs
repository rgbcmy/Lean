using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Backtest configuration and results / 回测配置和结果
    /// </summary>
    public class Backtest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int StrategyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialCapital { get; set; }

        [MaxLength(50)]
        public string? BenchmarkSymbol { get; set; }

        [Required]
        [MaxLength(50)]
        public string DataResolution { get; set; } = "Daily"; // Daily, Hourly, Minute

        public string? ParametersJson { get; set; } // Additional strategy parameters

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int? Progress { get; set; } // 0-100 percentage

        public string? ErrorMessage { get; set; }

        // Results
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TotalReturn { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? AnnualReturn { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? SharpeRatio { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? MaxDrawdown { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? WinRate { get; set; }

        public int? TotalTrades { get; set; }

        public int? WinningTrades { get; set; }

        public int? LosingTrades { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? AverageWin { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? AverageLoss { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ProfitFactor { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? Alpha { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? Beta { get; set; }

        public string? EquityCurveJson { get; set; } // JSON array of equity values

        public string? DrawdownCurveJson { get; set; } // JSON array of drawdown values

        public string? TradesJson { get; set; } // JSON array of trades

        public string? ResultsJson { get; set; } // Full Lean backtest results

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(StrategyId))]
        public virtual Strategy Strategy { get; set; } = null!;
    }
}
