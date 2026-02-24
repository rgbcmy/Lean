using System;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Data.Entities
{
    /// <summary>
    /// Key-value store for persisting system settings (theme, language, IBKR config, etc.)
    /// </summary>
    public class SystemSetting
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
