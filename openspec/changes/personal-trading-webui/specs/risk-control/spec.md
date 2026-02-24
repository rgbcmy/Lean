# Spec: Risk Control（风险控制）

## ADDED Requirements

### Requirement: Configure stop-loss rules
系统必须支持配置止损规则。

#### Scenario: Set percentage stop-loss
- **WHEN** 用户为某个持仓设置 5% 止损
- **THEN** 系统监控该持仓，当跌幅达到 5% 时自动触发卖出

#### Scenario: Set dollar amount stop-loss
- **WHEN** 用户设置 $500 止损
- **THEN** 系统在亏损达到 $500 时自动平仓

#### Scenario: Trailing stop-loss
- **WHEN** 用户设置 3% 移动止损
- **THEN** 系统随价格上涨动态调整止损价（始终保持 3% 距离）

#### Scenario: Disable stop-loss
- **WHEN** 用户取消止损设置
- **THEN** 系统停止监控该持仓的止损规则

### Requirement: Configure take-profit rules
系统必须支持配置止盈规则。

#### Scenario: Set percentage take-profit
- **WHEN** 用户为某个持仓设置 10% 止盈
- **THEN** 系统监控该持仓，当涨幅达到 10% 时自动触发卖出

#### Scenario: Partial take-profit
- **WHEN** 用户设置分批止盈（如 50% 仓位在 +10%、剩余在 +20%）
- **THEN** 系统在达到目标时分批平仓

#### Scenario: Notify before taking profit
- **WHEN** 持仓接近止盈目标（如还差 1%）
- **THEN** 系统发送提示"[股票代码] 接近止盈目标"

### Requirement: Enforce position size limits
系统必须限制单个持仓的大小。

#### Scenario: Maximum position size
- **WHEN** 用户设置单个股票最大持仓为账户总值的 20%
- **THEN** 系统拒绝超过该限制的买入订单

#### Scenario: Maximum number of positions
- **WHEN** 用户设置最多持有 10 只股票
- **THEN** 系统在已有 10 个持仓时拒绝新的买入订单

#### Scenario: Minimum cash reserve
- **WHEN** 用户设置最低现金比例为 10%
- **THEN** 系统确保账户始终保留至少 10% 的现金

### Requirement: Enforce daily trading limits
系统必须限制日内交易频率。

#### Scenario: Maximum trades per day
- **WHEN** 用户设置每日最多 20 笔交易
- **THEN** 系统在达到 20 笔后拒绝当日新订单

#### Scenario: Maximum order value per day
- **WHEN** 用户设置每日最大交易额为 $50,000
- **THEN** 系统累计当日交易金额，超过限制时阻止下单

#### Scenario: Cooldown period
- **WHEN** 用户设置同一股票两次交易间隔至少 10 分钟
- **THEN** 系统在 10 分钟内拒绝同一股票的重复交易

### Requirement: Detect and alert on margin calls
系统必须监控保证金使用情况。

#### Scenario: Margin usage warning
- **WHEN** 保证金使用率超过 80%
- **THEN** 系统发送警告通知"保证金使用率过高"

#### Scenario: Prevent new positions on high margin
- **WHEN** 保证金使用率超过 90%
- **THEN** 系统阻止新的买入订单，仅允许平仓

#### Scenario: Margin call alert
- **WHEN** 收到券商的追加保证金通知
- **THEN** 系统立即发送紧急告警"收到追加保证金通知"

### Requirement: Monitor portfolio concentration risk
系统必须监控投资组合的集中度风险。

#### Scenario: Sector concentration warning
- **WHEN** 某个行业占比超过 40%
- **THEN** 系统显示警告"科技行业集中度过高（45%）"

#### Scenario: Single stock concentration warning
- **WHEN** 单个股票占比超过 30%
- **THEN** 系统显示警告"[股票代码] 占比过高，建议分散投资"

#### Scenario: Suggest rebalancing
- **WHEN** 检测到集中度不均衡
- **THEN** 系统提供再平衡建议（建议卖出/买入哪些股票）

### Requirement: Implement circuit breakers
系统必须实施熔断机制防止异常交易。

#### Scenario: Rapid price movement halt
- **WHEN** 系统检测到某股票价格在 1 分钟内波动超过 5%
- **THEN** 暂停该股票的交易 5 分钟并发送告警

#### Scenario: Unusual order volume
- **WHEN** 系统检测到订单数量异常（如 1 分钟内 100 笔）
- **THEN** 暂停自动交易并要求人工确认

#### Scenario: Large order confirmation
- **WHEN** 订单金额超过 $10,000
- **THEN** 系统要求二次确认："确认提交大额订单？"

### Requirement: Track and display risk metrics
系统必须计算并显示风险指标。

#### Scenario: Display Value at Risk (VaR)
- **WHEN** 用户查看风险指标
- **THEN** 系统显示 VaR（95% 置信度下的最大可能损失）

#### Scenario: Display portfolio volatility
- **WHEN** 用户查看波动率
- **THEN** 系统显示投资组合的年化波动率

#### Scenario: Display Sharpe ratio
- **WHEN** 用户查看风险调整后收益
- **THEN** 系统显示夏普比率

#### Scenario: Display maximum drawdown
- **WHEN** 用户查看历史最大损失
- **THEN** 系统显示最大回撤百分比和发生时间

### Requirement: Enforce day trading rules
系统必须遵守美国日内交易规则（Pattern Day Trader）。

#### Scenario: Detect day trade
- **WHEN** 用户当日买卖同一股票
- **THEN** 系统标记为一次日内交易

#### Scenario: Count day trades in rolling 5 days
- **WHEN** 系统检测到 5 个交易日内发生 4 次日内交易
- **THEN** 显示警告"您已被标记为日内交易者，需要满足 $25,000 最低资金要求"

#### Scenario: Block day trade if insufficient capital
- **WHEN** 账户资金低于 $25,000 且已有 3 次日内交易
- **THEN** 系统阻止第 4 次日内交易

### Requirement: Provide risk alerts and notifications
系统必须及时发送风险告警。

#### Scenario: Email alert on high risk
- **WHEN** 触发高风险事件（如止损、追加保证金）
- **THEN** 系统发送邮件告警到用户注册邮箱

#### Scenario: Dashboard risk indicator
- **WHEN** 存在风险警告
- **THEN** 系统在仪表板显示红色警告图标和摘要

#### Scenario: Risk level color coding
- **WHEN** 用户查看风险等级
- **THEN** 系统用绿色（低）、黄色（中）、红色（高）显示风险等级

### Requirement: Generate risk reports
系统必须生成风险分析报告。

#### Scenario: Daily risk report
- **WHEN** 每日收盘后
- **THEN** 系统自动生成当日风险报告（持仓风险、集中度、VaR）

#### Scenario: Export risk report
- **WHEN** 用户导出风险报告
- **THEN** 系统生成 PDF 文件（包含所有风险指标和图表）

#### Scenario: Historical risk comparison
- **WHEN** 用户查看风险趋势
- **THEN** 系统显示风险指标的历史变化曲线
