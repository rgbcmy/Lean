using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Order history and tracking
    /// </summary>
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BrokerAccountId { get; set; }

        public int? StrategyExecutionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BrokerOrderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string OrderType { get; set; } = "Market"; // Market, Limit, Stop, StopLimit

        [Required]
        [MaxLength(10)]
        public string Side { get; set; } = "Buy"; // Buy, Sell

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LimitPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? StopPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? FilledPrice { get; set; }

        public int FilledQuantity { get; set; } = 0;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, PartiallyFilled, Filled, Cancelled, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? FilledAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        // Navigation properties
        [ForeignKey(nameof(BrokerAccountId))]
        public virtual BrokerAccount BrokerAccount { get; set; } = null!;
    }
}
