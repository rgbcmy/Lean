# Spec: US Stock Trading（美股股票交易）

## ADDED Requirements

### Requirement: Search for US stocks
系统必须提供美股股票搜索功能。

#### Scenario: Search by ticker symbol
- **WHEN** 用户输入股票代码（如"AAPL"）
- **THEN** 系统返回匹配的股票信息（名称、交易所、最新价）

#### Scenario: Search by company name
- **WHEN** 用户输入公司名称（如"Apple"）
- **THEN** 系统返回相关股票列表（支持模糊匹配）

#### Scenario: No matching results
- **WHEN** 用户输入不存在的股票代码
- **THEN** 系统显示"未找到匹配的股票"提示

### Requirement: Place market order
系统必须支持市价单下单。

#### Scenario: Buy market order
- **WHEN** 用户选择买入、输入股票代码和数量、选择市价单
- **THEN** 系统提交买入市价单到 IBKR 并返回订单 ID

#### Scenario: Sell market order
- **WHEN** 用户选择卖出、输入股票代码和数量、选择市价单
- **THEN** 系统提交卖出市价单到 IBKR 并返回订单 ID

#### Scenario: Insufficient buying power
- **WHEN** 用户尝试买入但账户购买力不足
- **THEN** 系统拒绝订单并显示"购买力不足，当前可用：$X"

#### Scenario: Insufficient shares to sell
- **WHEN** 用户尝试卖出超过持仓数量的股票
- **THEN** 系统拒绝订单并显示"持仓不足，当前持有：X 股"

### Requirement: Place limit order
系统必须支持限价单下单。

#### Scenario: Buy limit order
- **WHEN** 用户选择买入、输入股票代码、数量、限价价格
- **THEN** 系统提交买入限价单并显示"限价单已提交"

#### Scenario: Sell limit order
- **WHEN** 用户选择卖出、输入股票代码、数量、限价价格
- **THEN** 系统提交卖出限价单并显示"限价单已提交"

#### Scenario: Invalid limit price
- **WHEN** 用户输入负数或零作为限价
- **THEN** 系统显示错误"限价必须大于 0"

### Requirement: Cancel pending order
系统必须允许用户取消未成交的订单。

#### Scenario: Cancel unfilled order
- **WHEN** 用户选择一个未成交的订单并点击"取消"
- **THEN** 系统发送取消请求到 IBKR 并更新订单状态为"已取消"

#### Scenario: Cancel partially filled order
- **WHEN** 用户取消部分成交的订单
- **THEN** 系统取消剩余未成交部分并保留已成交部分

#### Scenario: Order already filled
- **WHEN** 用户尝试取消已完全成交的订单
- **THEN** 系统显示"订单已完全成交，无法取消"

### Requirement: View order status
系统必须显示订单的实时状态。

#### Scenario: Order submitted
- **WHEN** 订单刚提交到 IBKR
- **THEN** 系统显示订单状态为"已提交"（Submitted）

#### Scenario: Order partially filled
- **WHEN** 订单部分成交
- **THEN** 系统显示"部分成交：已成交 X / 总量 Y"

#### Scenario: Order filled
- **WHEN** 订单完全成交
- **THEN** 系统显示订单状态为"已成交"并显示成交均价

#### Scenario: Order cancelled
- **WHEN** 订单被用户或系统取消
- **THEN** 系统显示订单状态为"已取消"

#### Scenario: Order rejected
- **WHEN** IBKR 拒绝订单（如市场关闭、价格异常）
- **THEN** 系统显示订单状态为"已拒绝"并显示拒绝原因

### Requirement: Display real-time order updates
系统必须实时推送订单状态变化。

#### Scenario: Order status change notification
- **WHEN** 订单状态从"已提交"变为"已成交"
- **THEN** 系统通过 SignalR 立即推送更新到前端，前端显示通知

#### Scenario: Order fill notification
- **WHEN** 订单成交（全部或部分）
- **THEN** 系统推送成交通知，包括成交数量、价格、时间

### Requirement: View order history
系统必须提供订单历史查询功能。

#### Scenario: Query today's orders
- **WHEN** 用户查看今日订单
- **THEN** 系统显示当日所有订单（包括已成交、已取消、进行中）

#### Scenario: Query historical orders
- **WHEN** 用户选择日期范围查询历史订单
- **THEN** 系统从数据库查询并显示该时间段的所有订单

#### Scenario: Filter by stock symbol
- **WHEN** 用户输入股票代码筛选订单
- **THEN** 系统仅显示该股票的订单记录

#### Scenario: Filter by order status
- **WHEN** 用户选择状态筛选（如"已成交"）
- **THEN** 系统仅显示匹配状态的订单

### Requirement: Validate order parameters
系统必须在提交订单前验证参数。

#### Scenario: Invalid quantity
- **WHEN** 用户输入小于 1 的股票数量
- **THEN** 系统显示错误"数量必须至少为 1"

#### Scenario: Fractional shares not allowed
- **WHEN** 用户输入小数数量（如 1.5 股）
- **THEN** 系统显示错误"不支持碎股交易，请输入整数"

#### Scenario: Market closed warning
- **WHEN** 用户在美股休市时下单
- **THEN** 系统显示警告"市场已关闭，订单将在下一交易日开盘时执行"

### Requirement: Display order confirmation
系统必须在提交订单前显示确认对话框。

#### Scenario: Confirm market order
- **WHEN** 用户提交市价单
- **THEN** 系统显示确认框："确认以市价买入/卖出 [数量] 股 [股票代码]？"

#### Scenario: Confirm limit order
- **WHEN** 用户提交限价单
- **THEN** 系统显示确认框："确认以限价 $[价格] 买入/卖出 [数量] 股 [股票代码]？"

#### Scenario: User cancels confirmation
- **WHEN** 用户在确认框中点击"取消"
- **THEN** 系统不提交订单并返回交易界面

### Requirement: Calculate estimated order cost
系统必须显示订单的预估成本。

#### Scenario: Estimate market buy cost
- **WHEN** 用户准备提交买入市价单
- **THEN** 系统显示预估成本："约 $[最新价 × 数量] + 佣金"

#### Scenario: Estimate limit buy cost
- **WHEN** 用户准备提交买入限价单
- **THEN** 系统显示预估成本："最多 $[限价 × 数量] + 佣金"

#### Scenario: Display commission estimate
- **WHEN** 系统计算订单成本
- **THEN** 显示 IBKR 佣金预估（依据账户类型和地区）
