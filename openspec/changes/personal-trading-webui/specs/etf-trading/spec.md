# Spec: ETF Trading（ETF 和指数基金交易）

## ADDED Requirements

### Requirement: Search for ETFs
系统必须提供 ETF 搜索和筛选功能。

#### Scenario: Search by ETF ticker
- **WHEN** 用户输入 ETF 代码（如"SPY"）
- **THEN** 系统返回 ETF 信息（名称、追踪指数、最新价、费率）

#### Scenario: Filter by category
- **WHEN** 用户选择 ETF 类别（如"大盘指数"、"科技"、"债券"）
- **THEN** 系统显示该类别下的所有 ETF 列表

#### Scenario: Sort by expense ratio
- **WHEN** 用户按费率排序
- **THEN** 系统从低到高显示 ETF（费率越低越靠前）

### Requirement: Display ETF details
系统必须显示 ETF 的详细信息。

#### Scenario: View basic ETF information
- **WHEN** 用户查看某个 ETF
- **THEN** 系统显示名称、追踪指数、发行商、资产规模、费率

#### Scenario: View ETF holdings
- **WHEN** 用户查看 ETF 持仓
- **THEN** 系统显示前 10 大持仓股票及其权重

#### Scenario: View ETF performance
- **WHEN** 用户查看 ETF 表现
- **THEN** 系统显示日涨跌、周/月/年收益率

### Requirement: Place ETF orders
系统必须支持 ETF 的交易订单（复用股票交易接口）。

#### Scenario: Buy ETF with market order
- **WHEN** 用户买入 ETF 并选择市价单
- **THEN** 系统提交 ETF 买入市价单，流程与股票相同

#### Scenario: Sell ETF with limit order
- **WHEN** 用户卖出 ETF 并选择限价单
- **THEN** 系统提交 ETF 卖出限价单，流程与股票相同

### Requirement: Support automatic investing (定投)
系统必须支持 ETF 的定期定额投资功能。

#### Scenario: Create recurring buy plan
- **WHEN** 用户设置定投计划（每周/每月、金额、ETF 代码）
- **THEN** 系统保存定投计划并显示"定投计划已创建"

#### Scenario: Execute recurring buy
- **WHEN** 定投执行日期到达
- **THEN** 系统自动按计划买入指定金额的 ETF（市价单）

#### Scenario: Insufficient funds for recurring buy
- **WHEN** 定投执行时账户余额不足
- **THEN** 系统跳过本次定投并发送通知"定投失败：余额不足"

#### Scenario: Pause recurring buy plan
- **WHEN** 用户暂停定投计划
- **THEN** 系统停止自动执行，直到用户恢复

#### Scenario: Cancel recurring buy plan
- **WHEN** 用户删除定投计划
- **THEN** 系统永久删除该计划并发送确认通知

### Requirement: View ETF portfolio allocation
系统必须显示用户的 ETF 持仓分布。

#### Scenario: Pie chart by ETF type
- **WHEN** 用户查看 ETF 持仓分布
- **THEN** 系统显示饼图（股票型、债券型、商品型等）

#### Scenario: View sector exposure
- **WHEN** 用户查看行业暴露
- **THEN** 系统汇总所有持有 ETF 的行业权重并显示

### Requirement: Compare ETFs
系统必须支持 ETF 对比功能。

#### Scenario: Compare two ETFs
- **WHEN** 用户选择两个 ETF（如 SPY vs VOO）
- **THEN** 系统并排显示两者的费率、追踪误差、收益率

#### Scenario: Compare ETF performance chart
- **WHEN** 用户查看对比图表
- **THEN** 系统显示两个 ETF 的历史价格走势对比

### Requirement: Display dividend information
系统必须显示 ETF 的分红信息。

#### Scenario: View dividend history
- **WHEN** 用户查看 ETF 分红记录
- **THEN** 系统显示最近 12 个月的分红日期、金额

#### Scenario: View annualized dividend yield
- **WHEN** 用户查看 ETF 详情
- **THEN** 系统显示年化股息率（基于最近 12 个月分红）

#### Scenario: Receive dividend notification
- **WHEN** 持有的 ETF 发放分红
- **THEN** 系统发送通知"您持有的 [ETF] 收到分红 $[金额]"

### Requirement: Filter liquid ETFs
系统必须标识流动性良好的 ETF。

#### Scenario: Display average volume
- **WHEN** 用户浏览 ETF 列表
- **THEN** 系统显示每个 ETF 的日均成交量

#### Scenario: Warn about illiquid ETFs
- **WHEN** 用户尝试交易日均成交量低于 10 万股的 ETF
- **THEN** 系统显示警告"该 ETF 流动性较低，可能影响成交"
