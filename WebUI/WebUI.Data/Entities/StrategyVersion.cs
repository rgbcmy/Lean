using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Strategy version history for rollback and comparison
    /// </summary>
    public class StrategyVersion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StrategyId { get; set; }

        [Required]
        public int VersionNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? ChangeDescription { get; set; }

        public string? ConfigurationJson { get; set; }

        public string? CodeFilePath { get; set; }

        // Navigation properties
        [ForeignKey(nameof(StrategyId))]
        public virtual Strategy Strategy { get; set; } = null!;
    }
}
