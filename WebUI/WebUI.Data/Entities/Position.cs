using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Current portfolio positions
    /// </summary>
    public class Position
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BrokerAccountId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Symbol { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal AverageCost { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CurrentPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnrealizedPnL { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RealizedPnL { get; set; }

        public DateTime FirstPurchaseDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(BrokerAccountId))]
        public virtual BrokerAccount BrokerAccount { get; set; } = null!;
    }
}
