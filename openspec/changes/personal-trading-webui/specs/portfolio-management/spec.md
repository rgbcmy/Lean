# Spec: Portfolio Management（持仓管理）

## ADDED Requirements

### Requirement: View current positions
系统必须显示用户的当前持仓列表。

#### Scenario: Display all positions
- **WHEN** 用户打开持仓页面
- **THEN** 系统显示所有持仓（股票代码、数量、成本价、当前价、盈亏）

#### Scenario: Empty portfolio
- **WHEN** 用户没有任何持仓
- **THEN** 系统显示"暂无持仓"提示

#### Scenario: Real-time price updates
- **WHEN** 持仓股票的市场价格变化
- **THEN** 系统实时更新当前价格和盈亏数据

### Requirement: Calculate position P&L
系统必须计算每个持仓的盈亏。

#### Scenario: Display unrealized P&L
- **WHEN** 用户查看持仓
- **THEN** 系统显示未实现盈亏（当前价 - 成本价）× 数量

#### Scenario: Display P&L percentage
- **WHEN** 用户查看持仓
- **THEN** 系统显示盈亏百分比 [(当前价 - 成本价) / 成本价 × 100%]

#### Scenario: Sum total P&L
- **WHEN** 用户查看总持仓
- **THEN** 系统显示所有持仓的总盈亏汇总

### Requirement: Display position details
系统必须提供持仓的详细信息。

#### Scenario: View average cost
- **WHEN** 用户查看某个持仓详情
- **THEN** 系统显示平均成本价（考虑多次买入的加权平均）

#### Scenario: View holding period
- **WHEN** 用户查看持仓详情
- **THEN** 系统显示持有天数（最早买入日期到今天）

#### Scenario: View position value
- **WHEN** 用户查看持仓详情
- **THEN** 系统显示当前市值（当前价 × 数量）

### Requirement: Close position
系统必须支持一键平仓功能。

#### Scenario: Close entire position
- **WHEN** 用户点击"平仓"按钮
- **THEN** 系统弹出确认框："确认以市价卖出全部 X 股 [股票代码]？"

#### Scenario: Confirm close position
- **WHEN** 用户确认平仓
- **THEN** 系统提交市价卖单（数量 = 持仓数量）

#### Scenario: Partial close position
- **WHEN** 用户选择部分平仓并输入数量
- **THEN** 系统提交指定数量的卖单

### Requirement: Sort and filter positions
系统必须支持持仓排序和筛选。

#### Scenario: Sort by P&L
- **WHEN** 用户选择按盈亏排序
- **THEN** 系统按盈亏百分比从高到低排列持仓

#### Scenario: Sort by value
- **WHEN** 用户选择按市值排序
- **THEN** 系统按持仓市值从大到小排列

#### Scenario: Filter by symbol
- **WHEN** 用户输入股票代码筛选
- **THEN** 系统仅显示匹配的持仓

#### Scenario: Filter profitable positions
- **WHEN** 用户选择"仅显示盈利"筛选
- **THEN** 系统仅显示盈亏为正的持仓

### Requirement: Display portfolio allocation
系统必须显示持仓的资产配置。

#### Scenario: Pie chart by position
- **WHEN** 用户查看持仓分布
- **THEN** 系统显示饼图（每个持仓占总市值的百分比）

#### Scenario: Pie chart by sector
- **WHEN** 用户查看行业分布
- **THEN** 系统显示按行业分类的资产配置饼图

#### Scenario: Cash allocation
- **WHEN** 用户查看资产配置
- **THEN** 系统在图表中包含现金比例

### Requirement: Track position history
系统必须记录持仓的历史变化。

#### Scenario: View buy history
- **WHEN** 用户查看某个持仓的交易历史
- **THEN** 系统显示所有买入记录（日期、数量、价格）

#### Scenario: View sell history
- **WHEN** 用户查看某个持仓的交易历史
- **THEN** 系统显示所有卖出记录（日期、数量、价格、盈亏）

#### Scenario: Calculate realized P&L
- **WHEN** 用户查看已平仓的持仓
- **THEN** 系统显示已实现盈亏（卖出价 - 买入成本）

### Requirement: Export portfolio data
系统必须支持导出持仓数据。

#### Scenario: Export to CSV
- **WHEN** 用户点击"导出"按钮
- **THEN** 系统生成 CSV 文件（包含股票代码、数量、成本、当前价、盈亏）

#### Scenario: Export to Excel
- **WHEN** 用户选择导出为 Excel
- **THEN** 系统生成 XLSX 文件（包含格式化的数据和图表）

### Requirement: Display portfolio performance
系统必须显示投资组合的整体表现。

#### Scenario: Today's P&L
- **WHEN** 用户查看今日盈亏
- **THEN** 系统显示今日所有持仓的盈亏变化总和

#### Scenario: Total return
- **WHEN** 用户查看总收益率
- **THEN** 系统显示账户从开始至今的累计收益率

#### Scenario: Performance chart
- **WHEN** 用户查看收益曲线
- **THEN** 系统显示账户净值的历史走势图

### Requirement: Alert on significant position changes
系统必须在持仓发生重大变化时提醒用户。

#### Scenario: Position drops 10%
- **WHEN** 某个持仓当日跌幅超过 10%
- **THEN** 系统发送通知"[股票代码] 今日跌幅超过 10%"

#### Scenario: Position gains 20%
- **WHEN** 某个持仓总盈利超过 20%
- **THEN** 系统发送通知"[股票代码] 盈利已达 20%，考虑止盈？"
