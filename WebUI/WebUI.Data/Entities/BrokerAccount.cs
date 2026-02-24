using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Broker account configuration (e.g., IBKR)
    /// </summary>
    public class BrokerAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string BrokerName { get; set; } = "IBKR";

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string AccountType { get; set; } = "Paper"; // Paper, Live

        [MaxLength(500)]
        public string? EncryptedCredentials { get; set; }

        public bool IsConnected { get; set; } = false;

        public DateTime? LastConnectedAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CashBalance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyingPower { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
