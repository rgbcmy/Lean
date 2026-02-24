using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Strategy execution history and status
    /// </summary>
    public class StrategyExecution
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StrategyId { get; set; }

        public int? BrokerAccountId { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StoppedAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Running"; // Running, Stopped, Failed, Completed

        public int ProcessId { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        public string? LogFilePath { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InitialCapital { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FinalCapital { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalReturn { get; set; }

        public int OrdersExecuted { get; set; } = 0;

        // Navigation properties
        [ForeignKey(nameof(StrategyId))]
        public virtual Strategy Strategy { get; set; } = null!;
    }
}
