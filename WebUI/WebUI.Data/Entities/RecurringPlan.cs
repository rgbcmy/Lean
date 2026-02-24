using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data.Entities;

/// <summary>
/// Recurring investment plan entity / 定投计划实体
/// Represents automatic periodic investments in ETFs
/// 表示 ETF 的自动定期投资计划
/// </summary>
public class RecurringPlan
{
    /// <summary>
    /// Plan ID / 计划 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// User ID / 用户 ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// User navigation property / 用户导航属性
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    /// <summary>
    /// Broker account ID / 券商账户 ID
    /// </summary>
    [Required]
    public int BrokerAccountId { get; set; }

    /// <summary>
    /// Broker account navigation property / 券商账户导航属性
    /// </summary>
    [ForeignKey(nameof(BrokerAccountId))]
    public BrokerAccount BrokerAccount { get; set; } = null!;

    /// <summary>
    /// ETF symbol / ETF 代码
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// ETF name / ETF 名称
    /// </summary>
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Investment amount per period / 每期投资金额
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency / 货币
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Frequency: Weekly, BiWeekly, Monthly / 频率：每周、双周、每月
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Frequency { get; set; } = "Monthly";

    /// <summary>
    /// Start date / 开始日期
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date (optional) / 结束日期（可选）
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Next execution date / 下次执行日期
    /// </summary>
    [Required]
    public DateTime NextExecutionDate { get; set; }

    /// <summary>
    /// Status: Active, Paused, Completed, Cancelled / 状态：活跃、暂停、已完成、已取消
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Is active / 是否活跃
    /// </summary>
    public bool IsActive => Status == "Active";

    /// <summary>
    /// Total times executed / 已执行次数
    /// </summary>
    public int ExecutionCount { get; set; } = 0;

    /// <summary>
    /// Last execution date / 最后执行日期
    /// </summary>
    public DateTime? LastExecutionDate { get; set; }

    /// <summary>
    /// Last execution status / 最后执行状态
    /// </summary>
    [MaxLength(50)]
    public string? LastExecutionStatus { get; set; }

    /// <summary>
    /// Last execution message / 最后执行消息
    /// </summary>
    [MaxLength(500)]
    public string? LastExecutionMessage { get; set; }

    /// <summary>
    /// Notes / 备注
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Created at / 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated at / 更新时间
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
