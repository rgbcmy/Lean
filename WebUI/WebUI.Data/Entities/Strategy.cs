using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Trading strategy configuration
    /// </summary>
    public class Strategy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string StrategyType { get; set; } = string.Empty; // Custom, Template, etc.

        public string? ConfigurationJson { get; set; } // JSON configuration

        public string? CodeFilePath { get; set; }

        [MaxLength(200)]
        public string? Tags { get; set; } // Comma-separated tags

        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int Version { get; set; } = 1;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<StrategyExecution> Executions { get; set; } = new List<StrategyExecution>();
    }
}
