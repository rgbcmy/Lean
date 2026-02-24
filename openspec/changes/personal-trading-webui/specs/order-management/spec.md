# Spec: Order Management（订单管理）

## ADDED Requirements

### Requirement: View active orders
系统必须显示所有活跃订单（未成交或部分成交）。

#### Scenario: Display pending orders
- **WHEN** 用户打开订单页面
- **THEN** 系统显示所有状态为"已提交"或"部分成交"的订单

#### Scenario: No active orders
- **WHEN** 用户没有任何活跃订单
- **THEN** 系统显示"暂无活跃订单"提示

#### Scenario: Real-time order updates
- **WHEN** 订单状态发生变化
- **THEN** 系统通过 SignalR 实时更新前端订单列表

### Requirement: View order history
系统必须提供完整的订单历史记录查询。

#### Scenario: Today's orders
- **WHEN** 用户查看今日订单
- **THEN** 系统显示当天所有订单（所有状态）

#### Scenario: Historical orders with date range
- **WHEN** 用户选择日期范围（如"最近 7 天"）
- **THEN** 系统从数据库查询该时间段的订单记录

#### Scenario: Pagination for large history
- **WHEN** 订单历史超过 100 条
- **THEN** 系统分页显示（每页 50 条）

### Requirement: Display order details
系统必须显示订单的详细信息。

#### Scenario: Order basic info
- **WHEN** 用户点击某个订单
- **THEN** 系统显示订单详情（股票代码、方向、数量、价格、类型、状态）

#### Scenario: Order execution details
- **WHEN** 用户查看已成交订单
- **THEN** 系统显示成交时间、成交价、成交数量、佣金

#### Scenario: Order timestamps
- **WHEN** 用户查看订单详情
- **THEN** 系统显示订单提交时间、最后更新时间

### Requirement: Cancel active orders
系统必须支持批量取消订单。

#### Scenario: Cancel single order
- **WHEN** 用户选择一个订单并点击"取消"
- **THEN** 系统发送取消请求并更新订单状态为"已取消"

#### Scenario: Cancel all orders
- **WHEN** 用户点击"取消全部"按钮
- **THEN** 系统弹出确认框："确认取消所有活跃订单？"

#### Scenario: Confirm cancel all
- **WHEN** 用户确认取消全部
- **THEN** 系统逐个取消所有活跃订单并显示取消结果

#### Scenario: Failed to cancel order
- **WHEN** 取消请求失败（如订单已成交）
- **THEN** 系统显示错误"取消失败：订单已成交"

### Requirement: Filter and search orders
系统必须支持订单筛选和搜索。

#### Scenario: Filter by status
- **WHEN** 用户选择状态筛选（如"已成交"）
- **THEN** 系统仅显示该状态的订单

#### Scenario: Filter by symbol
- **WHEN** 用户输入股票代码筛选
- **THEN** 系统仅显示该股票的订单

#### Scenario: Filter by order type
- **WHEN** 用户选择订单类型筛选（如"限价单"）
- **THEN** 系统仅显示该类型的订单

#### Scenario: Filter by side
- **WHEN** 用户选择方向筛选（如"买入"）
- **THEN** 系统仅显示买入订单

### Requirement: Sort orders
系统必须支持订单排序。

#### Scenario: Sort by time
- **WHEN** 用户选择按时间排序
- **THEN** 系统按提交时间倒序显示订单（最新的在前）

#### Scenario: Sort by quantity
- **WHEN** 用户选择按数量排序
- **THEN** 系统按订单数量从大到小排列

#### Scenario: Sort by price
- **WHEN** 用户选择按价格排序
- **THEN** 系统按订单价格从高到低排列

### Requirement: Display order statistics
系统必须显示订单统计信息。

#### Scenario: Daily order count
- **WHEN** 用户查看今日统计
- **THEN** 系统显示今日订单数量、成交率

#### Scenario: Weekly trade summary
- **WHEN** 用户查看本周统计
- **THEN** 系统显示本周交易次数、总成交金额、平均订单大小

#### Scenario: Order success rate
- **WHEN** 用户查看订单成功率
- **THEN** 系统显示"已成交订单数 / 总订单数"百分比

### Requirement: Track order modifications
系统必须记录订单的修改历史（如果支持修改）。

#### Scenario: View modification history
- **WHEN** 用户查看已修改过的订单
- **THEN** 系统显示修改记录（原价格 → 新价格、修改时间）

#### Scenario: Original order preserved
- **WHEN** 订单被修改
- **THEN** 系统保留原始订单参数以供审计

### Requirement: Export order history
系统必须支持导出订单历史。

#### Scenario: Export to CSV
- **WHEN** 用户点击"导出订单"按钮
- **THEN** 系统生成 CSV 文件（包含日期、股票、方向、数量、价格、状态）

#### Scenario: Export filtered results
- **WHEN** 用户在筛选后导出
- **THEN** 系统仅导出当前筛选条件下的订单

### Requirement: Display order execution quality
系统必须显示订单执行质量指标。

#### Scenario: Price improvement
- **WHEN** 市价单成交价优于提交时的市场价
- **THEN** 系统显示价格改善金额（如"优于预期 $0.05/股"）

#### Scenario: Slippage
- **WHEN** 市价单成交价劣于提交时的市场价
- **THEN** 系统显示滑点金额（如"滑点 $0.10/股"）

#### Scenario: Fill time
- **WHEN** 用户查看订单执行详情
- **THEN** 系统显示从提交到完全成交的时长

### Requirement: Notify on order events
系统必须在订单关键事件发生时通知用户。

#### Scenario: Order filled notification
- **WHEN** 订单完全成交
- **THEN** 系统发送通知"订单已成交：[方向] [数量] 股 [股票代码] @ $[价格]"

#### Scenario: Order partially filled notification
- **WHEN** 订单部分成交
- **THEN** 系统发送通知"订单部分成交：已成交 [数量] / [总量]"

#### Scenario: Order rejected notification
- **WHEN** 订单被拒绝
- **THEN** 系统发送通知"订单被拒绝：[原因]"

#### Scenario: Order cancelled notification
- **WHEN** 用户取消订单成功
- **THEN** 系统发送通知"订单已取消：[股票代码]"
