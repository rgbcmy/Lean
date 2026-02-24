# Spec: Realtime Market Data（实时行情数据）

## ADDED Requirements

### Requirement: Subscribe to real-time quotes
系统必须支持订阅股票和 ETF 的实时行情。

#### Scenario: Subscribe to a stock
- **WHEN** 用户查看某只股票的详情页
- **THEN** 系统自动订阅该股票的实时行情并开始推送

#### Scenario: Unsubscribe when leaving page
- **WHEN** 用户离开股票详情页
- **THEN** 系统自动取消订阅以释放资源

#### Scenario: Multiple subscriptions
- **WHEN** 用户同时查看多只股票
- **THEN** 系统支持同时订阅最多 50 只股票的实时行情

### Requirement: Display Level 1 market data
系统必须显示 Level 1 行情数据（最新价、买卖价）。

#### Scenario: Display last price
- **WHEN** 收到新的成交价格
- **THEN** 系统更新并显示最新价格

#### Scenario: Display bid and ask
- **WHEN** 收到买卖盘报价
- **THEN** 系统显示最佳买价、卖价及其数量

#### Scenario: Display price change
- **WHEN** 股票价格变化
- **THEN** 系统显示涨跌额和涨跌幅（红涨绿跌，支持中美显示习惯切换）

#### Scenario: Display volume
- **WHEN** 收到成交量数据
- **THEN** 系统显示当日累计成交量

### Requirement: Push real-time updates via SignalR
系统必须通过 SignalR 推送实时行情到前端。

#### Scenario: Price update pushed
- **WHEN** 股票价格发生变化
- **THEN** 系统通过 SignalR 推送更新到所有订阅的客户端

#### Scenario: Throttle push frequency
- **WHEN** 股票价格频繁变化（如每秒 10 次）
- **THEN** 系统限制推送频率为最多每秒 1 次（合并更新）

#### Scenario: Push only subscribed symbols
- **WHEN** 客户端订阅了特定股票列表
- **THEN** 系统仅推送用户订阅的股票行情，不推送无关数据

### Requirement: Display market status
系统必须显示市场当前状态。

#### Scenario: Market is open
- **WHEN** 美股市场开盘时间（EST 9:30-16:00）
- **THEN** 系统显示"市场开盘中"状态指示器（绿色）

#### Scenario: Market is closed
- **WHEN** 美股市场休市时间
- **THEN** 系统显示"市场已休市"状态指示器（灰色）

#### Scenario: Pre-market trading
- **WHEN** 盘前交易时间（EST 4:00-9:30）
- **THEN** 系统显示"盘前交易"状态指示器（黄色）

#### Scenario: After-hours trading
- **WHEN** 盘后交易时间（EST 16:00-20:00）
- **THEN** 系统显示"盘后交易"状态指示器（黄色）

### Requirement: Display delayed quotes for unsubscribed data
系统必须为未订阅的用户提供延迟行情。

#### Scenario: Free delayed quotes
- **WHEN** 用户未订阅 IBKR 实时行情数据（需付费）
- **THEN** 系统显示 15 分钟延迟行情并标注"延迟 15 分钟"

#### Scenario: Upgrade to real-time quotes
- **WHEN** 用户点击"升级到实时行情"
- **THEN** 系统显示 IBKR 市场数据订阅指南链接

### Requirement: Cache recent market data
系统必须缓存最近的行情数据以减少 API 调用。

#### Scenario: Cache last price
- **WHEN** 收到新的行情数据
- **THEN** 系统缓存最新价格到 Redis（5 分钟过期）

#### Scenario: Serve from cache
- **WHEN** 用户请求行情但缓存未过期
- **THEN** 系统直接返回缓存数据，不调用 IBKR API

### Requirement: Display trading hours information
系统必须显示股票的交易时间信息。

#### Scenario: Regular trading hours
- **WHEN** 用户查看股票详情
- **THEN** 系统显示常规交易时间（EST 9:30-16:00）

#### Scenario: Extended hours indication
- **WHEN** 股票支持盘前盘后交易
- **THEN** 系统显示"支持盘前盘后交易"标识

### Requirement: Handle market data errors
系统必须优雅处理行情数据错误。

#### Scenario: Symbol not found
- **WHEN** 订阅不存在的股票代码
- **THEN** 系统显示"股票代码不存在"错误并取消订阅

#### Scenario: Market data permission denied
- **WHEN** IBKR 返回权限不足错误（未订阅该市场数据）
- **THEN** 系统显示"无权限访问该市场数据，请订阅"提示

#### Scenario: Connection lost during streaming
- **WHEN** 行情推送过程中连接断开
- **THEN** 系统显示"行情连接已断开"警告并尝试重连

### Requirement: Display market summary
系统必须显示市场概览信息。

#### Scenario: Index overview
- **WHEN** 用户打开首页或市场页面
- **THEN** 系统显示主要指数（标普 500、纳斯达克、道琼斯）的实时价格和涨跌

#### Scenario: Market movers
- **WHEN** 用户查看市场动态
- **THEN** 系统显示涨幅前 10 和跌幅前 10 的股票

#### Scenario: Sector performance
- **WHEN** 用户查看行业表现
- **THEN** 系统显示 11 个主要行业（科技、金融、医疗等）的今日涨跌幅

### Requirement: Support multiple symbol formats
系统必须支持多种股票代码格式。

#### Scenario: US stock ticker
- **WHEN** 用户输入"AAPL"
- **THEN** 系统识别为纳斯达克的苹果公司股票

#### Scenario: Full symbol with exchange
- **WHEN** 用户输入"AAPL@NASDAQ"
- **THEN** 系统明确订阅纳斯达克交易所的 AAPL

#### Scenario: Case insensitive
- **WHEN** 用户输入"aapl"或"Aapl"
- **THEN** 系统自动转换为大写"AAPL"并正常订阅
