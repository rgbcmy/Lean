using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Parameter optimization record / 参数优化记录
    /// </summary>
    public class ParameterOptimization
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

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialCapital { get; set; }

        [Required]
        public string ParameterRangesJson { get; set; } = string.Empty; // JSON array of parameter ranges

        [Required]
        [MaxLength(50)]
        public string OptimizationMetric { get; set; } = "SharpeRatio";

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed

        public int TotalCombinations { get; set; }

        public int CompletedCombinations { get; set; }

        public int? BestBacktestId { get; set; }

        public string? ResultsJson { get; set; } // JSON array of optimization results

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(StrategyId))]
        public virtual Strategy Strategy { get; set; } = null!;

        [ForeignKey(nameof(BestBacktestId))]
        public virtual Backtest? BestBacktest { get; set; }
    }
}
